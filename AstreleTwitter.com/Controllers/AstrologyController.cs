using AstreleTwitter.com.Models;
using AstreleTwitter.com.Services;
using Microsoft.AspNetCore.Mvc;

namespace AstreleTwitter.com.Controllers
{
    public class AstrologyController : Controller
    {
        private readonly AstrologyService _astrologyService;

        public AstrologyController(AstrologyService astrologyService)
        {
            _astrologyService = astrologyService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new AstrologyViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> GetHoroscope(DateTime birthDate)
        {
            var model = new AstrologyViewModel
            {
                BirthDate = birthDate,
                HasData = true
            };

            string signKey = _astrologyService.GetZodiacSignKey(birthDate);

            model.ZodiacSign = _astrologyService.GetRomanianSignName(signKey);

            model.HoroscopeText = await _astrologyService.GetDailyHoroscope(signKey);

            ViewBag.SignKey = signKey;

            return View("Index", model);
        }
    }
}