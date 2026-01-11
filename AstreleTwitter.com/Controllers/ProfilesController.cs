using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AstreleTwitter.com.Data;
using AstreleTwitter.com.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace AstreleTwitter.com.Controllers
{
    public class ProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly AstreleTwitter.com.Services.GeminiModerationService _moderationService; 

        public ProfilesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env, AstreleTwitter.com.Services.GeminiModerationService moderationService)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _moderationService = moderationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var users = from u in _context.Users select u;
            if (currentUser != null)
            {
                users = users.Where(u => u.Id != currentUser.Id);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.FirstName.Contains(searchString)
                                      || u.LastName.Contains(searchString)
                                      || (u.FirstName + " " + u.LastName).Contains(searchString)
                                      || u.UserName.Contains(searchString));
            }

            return View(await users.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Show(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var userProfile = _context.Users
                                    .Include(u => u.Posts)
                                    .FirstOrDefault(u => u.Id == id);

            if (userProfile == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.CurrentUser = currentUser?.Id;
            ViewBag.IsAdmin = User.IsInRole("Admin");

            var followersList = _context.Followings
                .Where(f => f.FollowingId == id && f.Status == "Accepted")
                .Include(f => f.Follower)
                .ToList();

            var followingList = _context.Followings
                .Where(f => f.FollowerId == id && f.Status == "Accepted")
                .Include(f => f.FollowingUser)
                .ToList();

            ViewBag.FollowersList = followersList;
            ViewBag.FollowingList = followingList;

            ViewBag.FollowersCount = followersList.Count;
            ViewBag.FollowingCount = followingList.Count;

            bool isFollowing = false;
            string followStatus = "None";

            if (currentUser != null)
            {
                var rel = _context.Followings
                    .FirstOrDefault(f => f.FollowerId == currentUser.Id && f.FollowingId == id);

                if (rel != null)
                {
                    isFollowing = true;
                    followStatus = rel.Status;
                }

                if (currentUser.Id == id)
                {
                    ViewBag.PendingRequests = _context.Followings
                        .Include(f => f.Follower)
                        .Where(f => f.FollowingId == currentUser.Id && f.Status == "Pending")
                        .ToList();
                }
            }

            ViewBag.IsFollowing = isFollowing;
            ViewBag.FollowStatus = followStatus;

            bool canViewFull = !userProfile.AccountPrivacy ||
                               (currentUser != null && currentUser.Id == id) ||
                               User.IsInRole("Admin") ||
                               (isFollowing && followStatus == "Accepted");

            ViewBag.CanViewFull = canViewFull;

            return View(userProfile);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            return View(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(ApplicationUser model, IFormFile? profileImage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            bool hasError = false;

            var namePattern = @"^[\p{L}\s]+$";

            if (string.IsNullOrWhiteSpace(model.FirstName))
            {
                TempData["FirstNameError"] = "Prenumele este obligatoriu!";
                hasError = true;
            }
            else if (!Regex.IsMatch(model.FirstName, namePattern))
            {
                TempData["FirstNameError"] = "Prenumele poate conține doar litere și spații!";
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(model.LastName))
            {
                TempData["LastNameError"] = "Numele de familie este obligatoriu!";
                hasError = true;
            }
            else if (!Regex.IsMatch(model.LastName, namePattern))
            {
                TempData["LastNameError"] = "Numele poate conține doar litere și spații!";
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(model.Bio))
            {
                TempData["BioError"] = "Descrierea (Bio) este obligatorie!";
                hasError = true;
            }

            if (string.IsNullOrEmpty(user.ProfilePicture) && (profileImage == null || profileImage.Length == 0))
            {
                TempData["ProfilePictureError"] = "Trebuie să încarci o poză de profil!";
                hasError = true;
            }

            if (hasError)
            {
                model.ProfilePicture = user.ProfilePicture;
                return View(model);
            }

            var textToCheck = $"{model.FirstName} {model.LastName} {model.Bio}";
            bool isSafe = await _moderationService.IsContentSafe(textToCheck);

            if (!isSafe)
            {
                TempData["ErrorMessage"] = "Numele sau biografia conțin termeni nepotriviți. Te rugăm să reformulezi.";
                return RedirectToAction("Show", new { id = user.Id });
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Bio = model.Bio;

            if (model.AccountPrivacy == false && user.AccountPrivacy == true)
            {
                var pendingRequests = _context.Followings
                    .Where(f => f.FollowingId == user.Id && f.Status == "Pending")
                    .ToList();

                if (pendingRequests.Any())
                {
                    foreach (var req in pendingRequests)
                    {
                        req.Status = "Accepted";
                    }
                    _context.SaveChanges();
                }
            }

            user.AccountPrivacy = model.AccountPrivacy;

            if (profileImage != null && profileImage.Length > 0)
            {
                var storagePath = Path.Combine(_env.WebRootPath, "profiles");
                if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var filePath = Path.Combine(storagePath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                user.ProfilePicture = "/profiles/" + fileName;
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction("Show", new { id = user.Id });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminEdit(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminEdit(ApplicationUser model, IFormFile? profileImage, string? newPassword)
        {
            var user = await _context.Users.FindAsync(model.Id);
            if (user == null) return NotFound();

            var textToCheck = $"{model.FirstName} {model.LastName} {model.Bio}";
            bool isSafe = await _moderationService.IsContentSafe(textToCheck);

            if (!isSafe)
            {
                TempData["ErrorMessage"] = "Editarea respinsă: Numele sau biografia conțin termeni nepotriviți.";
                return RedirectToAction("Show", new { id = user.Id });
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Bio = model.Bio;

            if (user.Email != model.Email)
            {
                user.Email = model.Email;
                user.UserName = model.Email;
                user.NormalizedEmail = model.Email.ToUpper();
                user.NormalizedUserName = model.Email.ToUpper();
            }

            if (model.AccountPrivacy == false)
            {
                var pendingRequests = _context.Followings
                    .Where(f => f.FollowingId == user.Id && f.Status == "Pending")
                    .ToList();

                if (pendingRequests.Any())
                {
                    foreach (var req in pendingRequests)
                    {
                        req.Status = "Accepted";
                    }
                    _context.SaveChanges();
                }
            }
            user.AccountPrivacy = model.AccountPrivacy;

            if (profileImage != null && profileImage.Length > 0)
            {
                var storagePath = Path.Combine(_env.WebRootPath, "profiles");
                if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var filePath = Path.Combine(storagePath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                user.ProfilePicture = "/profiles/" + fileName;
            }
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return View(user);
            }

            if (!string.IsNullOrEmpty(newPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var passwordResult = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (!passwordResult.Succeeded)
                {

                }
            }

            return RedirectToAction("Show", new { id = user.Id });
        }
    }
}