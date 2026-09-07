using AstreleTwitter.com.Data;
using AstreleTwitter.com.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstreleTwitter.com.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly AstreleTwitter.com.Services.GeminiModerationService _moderationService; 

        public GroupsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env, AstreleTwitter.com.Services.GeminiModerationService moderationService)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _moderationService = moderationService;
        }

        public async Task<IActionResult> Index()
        {
            var groups = await _context.Groups
                                       .Include(g => g.UserGroups)
                                       .Include(g => g.Creator)
                                       .OrderByDescending(g => g.Date)
                                       .ToListAsync();
            return View(groups);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Group group, IFormFile? groupImage)
        {
            var user = await _userManager.GetUserAsync(User);
            group.CreatorId = user.Id;
            group.Date = DateTime.Now;

            if (groupImage != null && groupImage.Length > 0)
            {
                var storagePath = Path.Combine(_env.WebRootPath, "groups");
                if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(groupImage.FileName);
                var filePath = Path.Combine(storagePath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await groupImage.CopyToAsync(stream);
                }
                group.GroupImage = "/groups/" + fileName;
            }

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var userGroup = new UserGroup { GroupId = group.Id, UserId = user.Id, Status = "Accepted" };
            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();

            return RedirectToAction("Show", new { id = group.Id });
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);

            if (group == null) return NotFound();
            if (group.CreatorId != user.Id && !User.IsInRole("Admin")) return Forbid();

            return View(group);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Group requestGroup, IFormFile? groupImage, bool deleteImage)
        {
            var group = await _context.Groups.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);

            if (group == null) return NotFound();
            if (group.CreatorId != user.Id && !User.IsInRole("Admin")) return Forbid();

            group.Title = requestGroup.Title;
            group.Description = requestGroup.Description;

            if (deleteImage)
            {
                group.GroupImage = null;
            }
            else if (groupImage != null && groupImage.Length > 0)
            {
                var storagePath = Path.Combine(_env.WebRootPath, "groups");
                if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(groupImage.FileName);
                var filePath = Path.Combine(storagePath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await groupImage.CopyToAsync(stream);
                }
                group.GroupImage = "/groups/" + fileName;
            }

            _context.Update(group);
            await _context.SaveChangesAsync();
            return RedirectToAction("Show", new { id = group.Id });
        }

        public async Task<IActionResult> Show(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Creator)
                .Include(g => g.UserGroups).ThenInclude(ug => ug.User)
                .Include(g => g.Messages).ThenInclude(m => m.User)
                .Include(g => g.Messages).ThenInclude(m => m.GroupLikes)
                .Include(g => g.Messages).ThenInclude(m => m.GroupComments).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.CurrentUser = currentUser;

            bool isMember = false;
            string status = "Guest";
            bool isAdmin = User.IsInRole("Admin");

            if (currentUser != null)
            {
                var ug = group.UserGroups.FirstOrDefault(u => u.UserId == currentUser.Id);
                if (ug != null)
                {
                    status = ug.Status;
                    if (status == "Accepted") isMember = true;
                }
            }

            if (isAdmin) isMember = true;

            ViewBag.UserStatus = status;
            ViewBag.IsCreator = (currentUser != null && group.CreatorId == currentUser.Id) || isAdmin;
            ViewBag.IsMember = isMember;
            ViewBag.IsGlobalAdmin = isAdmin;

            return View(group);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Join(int groupId)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _context.UserGroups.FindAsync(user.Id, groupId);
            if (existing == null)
            {
                _context.UserGroups.Add(new UserGroup { UserId = user.Id, GroupId = groupId, Status = "Pending" });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Show", new { id = groupId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Leave(int groupId)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _context.UserGroups.FindAsync(user.Id, groupId);
            var group = await _context.Groups.FindAsync(groupId);

            if (group.CreatorId == user.Id && !User.IsInRole("Admin")) return Forbid();

            if (existing != null)
            {
                _context.UserGroups.Remove(existing);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AcceptMember(int groupId, string userId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            var currentUser = await _userManager.GetUserAsync(User);
            if (group.CreatorId != currentUser.Id && !User.IsInRole("Admin")) return Forbid();

            var ug = await _context.UserGroups.FindAsync(userId, groupId);
            if (ug != null)
            {
                ug.Status = "Accepted";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Show", new { id = groupId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RejectMember(int groupId, string userId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            var currentUser = await _userManager.GetUserAsync(User);
            if (group.CreatorId != currentUser.Id && !User.IsInRole("Admin")) return Forbid();

            var ug = await _context.UserGroups.FindAsync(userId, groupId);
            if (ug != null)
            {
                _context.UserGroups.Remove(ug);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Show", new { id = groupId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Messages)
                .Include(g => g.UserGroups)
                .FirstOrDefaultAsync(g => g.Id == id);

            var user = await _userManager.GetUserAsync(User);

            if (group != null && (group.CreatorId == user.Id || User.IsInRole("Admin")))
            {
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddMessage(int groupId, string content, IFormFile? mediaFile)
        {
            var user = await _userManager.GetUserAsync(User);
            var ug = await _context.UserGroups.FindAsync(user.Id, groupId);
            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin && (ug == null || ug.Status != "Accepted")) return Forbid();

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Nu poți trimite un mesaj gol în grup.";
                return RedirectToAction("Show", new { id = groupId });
            }


            bool isSafe = await _moderationService.IsContentSafe(content);
            if (!isSafe)
            {
                TempData["ErrorMessage"] = "Mesajul tău conține limbaj nepotrivit. Te rugăm să păstrezi un ton civilizat.";
                return RedirectToAction("Show", new { id = groupId });
            }

            var msg = new GroupMessage
            {
                Content = content,
                GroupId = groupId,
                UserId = user.Id,
                Date = DateTime.Now
            };

            if (mediaFile != null && mediaFile.Length > 0)
            {
                var storagePath = Path.Combine(_env.WebRootPath, "group_media");
                if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(mediaFile.FileName);
                var filePath = Path.Combine(storagePath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await mediaFile.CopyToAsync(stream);
                }
                msg.MediaPath = "/group_media/" + fileName;
            }

            _context.GroupMessages.Add(msg);
            await _context.SaveChangesAsync();

            return RedirectToAction("Show", new { id = groupId });
        }

        [Authorize]
        public async Task<IActionResult> EditMessage(int id)
        {
            var msg = await _context.GroupMessages.Include(m => m.Group).FirstOrDefaultAsync(m => m.Id == id);
            var user = await _userManager.GetUserAsync(User);

            if (msg == null) return NotFound();

            bool isAuthor = msg.UserId == user.Id;
            bool isModerator = msg.Group.CreatorId == user.Id;
            bool isAdmin = User.IsInRole("Admin");

            if (!isAuthor && !isModerator && !isAdmin)
            {
                return Forbid();
            }

            return View(msg);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditMessage(int id, string content)
        {
            var msg = await _context.GroupMessages.Include(m => m.Group).FirstOrDefaultAsync(m => m.Id == id);
            var user = await _userManager.GetUserAsync(User);

            if (msg == null) return NotFound();

            bool isAuthor = msg.UserId == user.Id;
            bool isModerator = msg.Group.CreatorId == user.Id;
            bool isAdmin = User.IsInRole("Admin");

            if (!isAuthor && !isModerator && !isAdmin)
            {
                return Forbid();
            }

            bool isSafe = await _moderationService.IsContentSafe(content);
            if (!isSafe)
            {
                TempData["ErrorMessage"] = "Editarea nu a fost salvată. Mesajul conține limbaj nepotrivit.";
                return RedirectToAction("Show", new { id = msg.GroupId });
            }

            msg.Content = content;
            _context.Update(msg);
            await _context.SaveChangesAsync();

            return RedirectToAction("Show", new { id = msg.GroupId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var msg = await _context.GroupMessages.Include(m => m.Group).FirstOrDefaultAsync(m => m.Id == messageId);
            var user = await _userManager.GetUserAsync(User);

            if (msg != null)
            {
                if (msg.UserId == user.Id || msg.Group.CreatorId == user.Id || User.IsInRole("Admin"))
                {
                    _context.GroupMessages.Remove(msg);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Show", new { id = msg.GroupId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleGroupLike(int messageId)
        {
            var user = await _userManager.GetUserAsync(User);
            var msg = await _context.GroupMessages.FindAsync(messageId);
            if (msg == null) return NotFound();

            var like = await _context.GroupLikes.FirstOrDefaultAsync(l => l.GroupMessageId == messageId && l.UserId == user.Id);

            if (like == null)
            {
                _context.GroupLikes.Add(new GroupLike { GroupMessageId = messageId, UserId = user.Id });
            }
            else
            {
                _context.GroupLikes.Remove(like);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Show", new { id = msg.GroupId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddGroupComment(int messageId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            var msg = await _context.GroupMessages.FindAsync(messageId);
            if (msg == null) return NotFound();

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Comentariul nu poate fi gol.";
                return RedirectToAction("Show", new { id = msg.GroupId });
            }

            bool isSafe = await _moderationService.IsContentSafe(content);
            if (!isSafe)
            {
                TempData["ErrorMessage"] = "Comentariul conține limbaj nepotrivit.";
                return RedirectToAction("Show", new { id = msg.GroupId });
            }

            var comm = new GroupComment
            {
                Content = content,
                GroupMessageId = messageId,
                UserId = user.Id,
                Date = DateTime.Now
            };
            _context.GroupComments.Add(comm);
            await _context.SaveChangesAsync();

            return RedirectToAction("Show", new { id = msg.GroupId });
        }

        [Authorize]
        public async Task<IActionResult> EditGroupComment(int id)
        {
            var comm = await _context.GroupComments
                .Include(c => c.GroupMessage)
                .ThenInclude(gm => gm.Group)
                .FirstOrDefaultAsync(c => c.Id == id);

            var user = await _userManager.GetUserAsync(User);

            if (comm == null) return NotFound();
            bool isAuthor = comm.UserId == user.Id;
            bool isModerator = comm.GroupMessage.Group.CreatorId == user.Id;
            bool isAdmin = User.IsInRole("Admin");

            if (!isAuthor && !isModerator && !isAdmin)
            {
                return Forbid();
            }

            return View(comm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditGroupComment(int id, string content)
        {
            var comm = await _context.GroupComments
                .Include(c => c.GroupMessage)
                .ThenInclude(gm => gm.Group)
                .FirstOrDefaultAsync(c => c.Id == id);

            var user = await _userManager.GetUserAsync(User);

            if (comm == null) return NotFound();

            bool isAuthor = comm.UserId == user.Id;
            bool isModerator = comm.GroupMessage.Group.CreatorId == user.Id;
            bool isAdmin = User.IsInRole("Admin");

            if (!isAuthor && !isModerator && !isAdmin)
            {
                return Forbid();
            }

            bool isSafe = await _moderationService.IsContentSafe(content);
            if (!isSafe)
            {
                TempData["ErrorMessage"] = "Editarea nu a fost salvată. Limbaj nepotrivit.";
                return RedirectToAction("Show", new { id = comm.GroupMessage.GroupId });
            }

            comm.Content = content;
            _context.Update(comm);
            await _context.SaveChangesAsync();

            return RedirectToAction("Show", new { id = comm.GroupMessage.GroupId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteGroupComment(int commentId)
        {
            var comm = await _context.GroupComments
                .Include(c => c.GroupMessage)
                .ThenInclude(gm => gm.Group)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            var user = await _userManager.GetUserAsync(User);

            if (comm != null)
            {
                bool isAuthor = comm.UserId == user.Id;
                bool isModerator = comm.GroupMessage.Group.CreatorId == user.Id;
                bool isAdmin = User.IsInRole("Admin");

                if (isAuthor || isModerator || isAdmin)
                {
                    _context.GroupComments.Remove(comm);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Show", new { id = comm?.GroupMessage.GroupId });
        }
    }
}