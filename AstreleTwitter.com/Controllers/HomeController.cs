using AstreleTwitter.com.Models;
using AstreleTwitter.com.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace AstreleTwitter.com.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index(string feedType = "global")
        {
            var currentUser = _userManager.GetUserAsync(User).Result;
            ViewBag.CurrentUser = currentUser;

            var myFollowingIds = new List<string>();
            if (currentUser != null)
            {
                myFollowingIds = _context.Followings
                    .Where(f => f.FollowerId == currentUser.Id && f.Status == "Accepted")
                    .Select(f => f.FollowingId)
                    .ToList();
            }
            ViewBag.MyFollowingIds = myFollowingIds;

            var postsQuery = _context.Posts
                            .Include(p => p.User)
                            .Include(p => p.Comments).ThenInclude(c => c.User)
                            .Include(p => p.Likes).ThenInclude(l => l.User)
                            .OrderByDescending(p => p.Date)
                            .AsQueryable();

            if (currentUser != null && feedType == "following")
            {
                var ids = new List<string>(myFollowingIds) { currentUser.Id };
                postsQuery = postsQuery.Where(p => ids.Contains(p.UserId));
            }

            ViewBag.FeedType = feedType;
            return View(postsQuery.ToList());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}