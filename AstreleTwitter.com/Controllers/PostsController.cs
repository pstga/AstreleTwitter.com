using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AstreleTwitter.com.Data;
using AstreleTwitter.com.Models;

namespace AstreleTwitter.com.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public PostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        private IActionResult RedirectToReferer()
        {
            var referer = Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(referer))
            {
                return RedirectToAction("Index", "Home");
            }
            return Redirect(referer);
        }

        [HttpPost]
        [Authorize]
        [RequestSizeLimit(100 * 1024 * 1024)]
        public IActionResult Create(string content, IFormFile? mediaFile)
        {
            try
            {
                var user = _userManager.GetUserAsync(User).Result;
                if (user == null) return RedirectToAction("Index", "Home");

                if (!string.IsNullOrWhiteSpace(content))
                {
                    var post = new Post
                    {
                        Content = content,
                        UserId = user.Id,
                        Date = DateTime.Now
                    };

                    if (mediaFile != null && mediaFile.Length > 0)
                    {
                        string rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var uploadDir = Path.Combine(rootPath, "uploads");

                        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(mediaFile.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            mediaFile.CopyTo(stream);
                        }

                        post.MediaPath = "/uploads/" + fileName;
                    }

                    _context.Posts.Add(post);
                    _context.SaveChanges();
                }
                return RedirectToReferer();
            }
            catch (Exception ex)
            {
                Console.WriteLine("EROARE: " + ex.Message);
                return RedirectToReferer();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Show(int id)
        {
            var post = await _context.Posts
                                    .Include(p => p.User)
                                    .Include(p => p.Comments).ThenInclude(c => c.User)
                                    .Include(p => p.Likes).ThenInclude(l => l.User)
                                    .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.CurrentUserId = _userManager.GetUserId(User);
            ViewBag.IsAdmin = User.IsInRole("Admin");

            return View(post);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            var post = _context.Posts.Find(id);
            var user = _userManager.GetUserAsync(User).Result;

            if (post == null || user == null || (post.UserId != user.Id && !User.IsInRole("Admin")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View(post);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Edit(int id, string content)
        {
            var post = _context.Posts.Find(id);
            var user = _userManager.GetUserAsync(User).Result;

            if (post != null && user != null)
            {
                if (User.IsInRole("Admin") || post.UserId == user.Id)
                {
                    post.Content = content;
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var post = _context.Posts.Find(id);
            var user = _userManager.GetUserAsync(User).Result;

            if (post != null && user != null && (User.IsInRole("Admin") || post.UserId == user.Id))
            {
                if (!string.IsNullOrEmpty(post.MediaPath))
                {
                    var filePath = Path.Combine(_env.WebRootPath, post.MediaPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Posts.Remove(post);
                _context.SaveChanges();
            }
            var referer = Request.Headers["Referer"].ToString();
            if (referer.Contains("Posts/Show")) return RedirectToAction("Index", "Home");

            return RedirectToReferer();
        }

        [HttpPost]
        [Authorize]
        public IActionResult DeleteMedia(int id)
        {
            var post = _context.Posts.Find(id);
            var user = _userManager.GetUserAsync(User).Result;

            if (post != null && user != null)
            {
                if (post.UserId == user.Id || User.IsInRole("Admin"))
                {
                    if (!string.IsNullOrEmpty(post.MediaPath))
                    {
                        string relativePath = post.MediaPath.TrimStart('/').TrimStart('\\');
                        var filePath = Path.Combine(_env.WebRootPath, relativePath);

                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    post.MediaPath = null;
                    _context.SaveChanges();
                }
            }
            return RedirectToReferer();
        }

        [HttpPost]
        [Authorize]
        public IActionResult AddComment(int postId, string content)
        {
            try
            {
                var user = _userManager.GetUserAsync(User).Result;
                if (user != null && !string.IsNullOrWhiteSpace(content))
                {
                    var comment = new Comment
                    {
                        PostId = postId,
                        Content = content,
                        UserId = user.Id,
                        Date = DateTime.Now
                    };
                    _context.Comments.Add(comment);
                    _context.SaveChanges();
                }
            }
            catch { }
            return RedirectToReferer();
        }

        [HttpGet]
        [Authorize]
        public IActionResult EditComment(int id)
        {
            var comm = _context.Comments.Find(id);
            var user = _userManager.GetUserAsync(User).Result;

            if (comm == null || user == null || (comm.UserId != user.Id && !User.IsInRole("Admin")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View(comm);
        }

        [HttpPost]
        [Authorize]
        public IActionResult EditComment(int id, string content)
        {
            var comm = _context.Comments.Find(id);
            var user = _userManager.GetUserAsync(User).Result;

            if (comm != null && user != null)
            {
                if (User.IsInRole("Admin") || comm.UserId == user.Id)
                {
                    comm.Content = content;
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        public IActionResult DeleteComment(int id)
        {
            var comm = _context.Comments.Find(id);
            var user = _userManager.GetUserAsync(User).Result;

            if (comm != null && user != null)
            {
                if (comm.UserId == user.Id || User.IsInRole("Admin"))
                {
                    _context.Comments.Remove(comm);
                    _context.SaveChanges();
                }
            }
            return RedirectToReferer();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleLike(int postId, string returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingLike = _context.Likes
                .FirstOrDefault(l => l.PostId == postId && l.UserId == user.Id);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
            }
            else
            {
                var like = new Like
                {
                    PostId = postId,
                    UserId = user.Id
                };
                _context.Likes.Add(like);
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}