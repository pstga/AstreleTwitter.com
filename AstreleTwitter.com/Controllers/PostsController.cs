using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

        [HttpPost]
        [Authorize]
        [RequestSizeLimit(100 * 1024 * 1024)]
        public async Task<IActionResult> Create(string content, IFormFile? mediaFile)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
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
                            await mediaFile.CopyToAsync(stream);
                        }

                        post.MediaPath = "/uploads/" + fileName;
                    }

                    _context.Posts.Add(post);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine("EROARE: " + ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);

            if (post == null || user == null || (post.UserId != user.Id && !User.IsInRole("Admin")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View(post);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, string content)
        {
            var post = await _context.Posts.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);

            if (post != null && user != null)
            {
                if (User.IsInRole("Admin") || post.UserId == user.Id)
                {
                    post.Content = content;
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);

            if (post != null && user != null && (User.IsInRole("Admin") || post.UserId == user.Id))
            {
                // Stergem si fisierul fizic daca exista
                if (!string.IsNullOrEmpty(post.MediaPath))
                {
                    var filePath = Path.Combine(_env.WebRootPath, post.MediaPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteMedia(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);

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
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Index", "Home");

                if (!string.IsNullOrWhiteSpace(content))
                {
                    var comment = new Comment
                    {
                        PostId = postId,
                        Content = content,
                        UserId = user.Id,
                        Date = DateTime.Now
                    };
                    _context.Comments.Add(comment);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eroare la comentariu: " + ex.Message);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditComment(int id)
        {
            var comm = await _context.Comments.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);

            if (comm == null || user == null || (comm.UserId != user.Id && !User.IsInRole("Admin")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View(comm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditComment(int id, string content)
        {
            var comm = await _context.Comments.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);

            if (comm != null && user != null)
            {
                if (User.IsInRole("Admin") || comm.UserId == user.Id)
                {
                    comm.Content = content;
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index", "Home");
        }

        // === 9. (NOU) STERGERE COMENTARIU ===
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comm = await _context.Comments.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);

            if (comm != null && user != null)
            {
                // Adminul sau Proprietarul COMENTARIULUI pot sterge
                if (comm.UserId == user.Id || User.IsInRole("Admin"))
                {
                    _context.Comments.Remove(comm);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}