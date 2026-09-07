using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Nodes;

namespace AstreleTwitter.com.Services
{
    public class AstrologyService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiModerationService _aiService;
        private readonly IMemoryCache _cache;

        public AstrologyService(HttpClient httpClient, GeminiModerationService aiService, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _aiService = aiService;
            _cache = cache;
        }

        public string GetZodiacSignKey(DateTime date)
        {
            int day = date.Day;
            int month = date.Month;

            if ((month == 3 && day >= 21) || (month == 4 && day <= 19)) return "aries";
            if ((month == 4 && day >= 20) || (month == 5 && day <= 20)) return "taurus";
            if ((month == 5 && day >= 21) || (month == 6 && day <= 20)) return "gemini";
            if ((month == 6 && day >= 21) || (month == 7 && day <= 22)) return "cancer";
            if ((month == 7 && day >= 23) || (month == 8 && day <= 22)) return "leo";
            if ((month == 8 && day >= 23) || (month == 9 && day <= 22)) return "virgo";
            if ((month == 9 && day >= 23) || (month == 10 && day <= 22)) return "libra";
            if ((month == 10 && day >= 23) || (month == 11 && day <= 21)) return "scorpio";
            if ((month == 11 && day >= 22) || (month == 12 && day <= 21)) return "sagittarius";
            if ((month == 12 && day >= 22) || (month == 1 && day <= 19)) return "capricorn";
            if ((month == 1 && day >= 20) || (month == 2 && day <= 18)) return "aquarius";
            if ((month == 2 && day >= 19) || (month == 3 && day <= 20)) return "pisces";

            return "aries";
        }

        public string GetRomanianSignName(string englishKey)
        {
            return englishKey.ToLower() switch
            {
                "aries" => "Berbec",
                "taurus" => "Taur",
                "gemini" => "Gemeni",
                "cancer" => "Rac",
                "leo" => "Leu",
                "virgo" => "Fecioară",
                "libra" => "Balanță",
                "scorpio" => "Scorpion",
                "sagittarius" => "Săgetător",
                "capricorn" => "Capricorn",
                "aquarius" => "Vărsător",
                "pisces" => "Pești",
                _ => "Necunoscut"
            };
        }

        public async Task<string> GetDailyHoroscope(string sign)
        {
            string cacheKey = $"Horoscope_{sign}_{DateTime.Today.ToShortDateString()}";

            if (_cache.TryGetValue(cacheKey, out string? cachedHoroscope))
            {
                return cachedHoroscope!;
            }

            try
            {
                var url = $"https://horoscope-app-api.vercel.app/api/v1/get-horoscope/daily?sign={sign}&day=today";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var jsonNode = JsonNode.Parse(jsonString);
                    var englishText = jsonNode?["data"]?["horoscope_data"]?.ToString();

                    if (!string.IsNullOrEmpty(englishText))
                    {
                        string translatedText = await _aiService.TranslateToRomanian(englishText);

                        _cache.Set(cacheKey, translatedText, TimeSpan.FromHours(12));

                        return translatedText;
                    }
                }
            }
            catch
            {
                return "Serviciul de horoscop este momentan indisponibil.";
            }

            return "Nu s-au putut prelua datele.";
        }
    }
}