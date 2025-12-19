using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AstreleTwitter.com.Data;
using AstreleTwitter.com.Models;
using Microsoft.AspNetCore.Authorization;

namespace AstreleTwitter.com.Controllers
{
    [Authorize]
    public class FollowsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public IActionResult ToggleFollow(string userId)
        {
            var currentUser = _userManager.GetUserAsync(User).Result;
            if (currentUser == null) return RedirectToAction("Index", "Home");

            var targetUser = _context.Users.Find(userId);
            if (targetUser == null || targetUser.Id == currentUser.Id) return RedirectToAction("Index", "Home");

            var existingFollow = _context.Followings.Find(currentUser.Id, userId);

            if (existingFollow != null)
            {
                _context.Followings.Remove(existingFollow);
            }
            else
            {
                var newFollow = new Following
                {
                    FollowerId = currentUser.Id,
                    FollowingId = userId,
                    Date = DateTime.Now,
                    Status = targetUser.AccountPrivacy ? "Pending" : "Accepted"
                };
                _context.Followings.Add(newFollow);
            }

            _context.SaveChanges();

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public IActionResult Accept(string followerId)
        {
            var currentUser = _userManager.GetUserAsync(User).Result;

            var request = _context.Followings.Find(followerId, currentUser.Id);

            if (request != null)
            {
                request.Status = "Accepted";
                _context.SaveChanges();
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public IActionResult Decline(string followerId)
        {
            var currentUser = _userManager.GetUserAsync(User).Result;

            var request = _context.Followings.Find(followerId, currentUser.Id);

            if (request != null)
            {
                _context.Followings.Remove(request);
                _context.SaveChanges();
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}