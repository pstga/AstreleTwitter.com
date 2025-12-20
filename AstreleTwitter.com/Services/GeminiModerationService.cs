using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AstreleTwitter.com.Services
{
    public class GeminiModerationService
    {
        // ---------------------------------------------------------
        // VERIFICA DACA AI CHEIA API CORECTA AICI!
        // ---------------------------------------------------------
        private readonly string _apiKey = "AIzaSyC4b9GV5-j8_PFjHT7zRQVhKVXdLG6Qem8";
        private readonly HttpClient _httpClient;

        private const string MODEL_NAME = "gemini-flash-lite-latest";

        public GeminiModerationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // === METODA 1: MODERARE CONTINUT ===
        public async Task<bool> IsContentSafe(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return true;

            var prompt = $@"
Role: Content Moderator.
Task: Check if the text contains insults, hate speech, swearing, or rude behavior (Romanian or English).
Rules:
- Even mild insults like 'prost', 'tampit', 'idiot', 'nebun', 'urat' MUST be marked as UNSAFE.
- Hate speech or violence is UNSAFE.
- Reply with JSON ONLY: {{ ""is_safe"": false }} if bad, {{ ""is_safe"": true }} if good.
Text: ""{text}""
";
            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{MODEL_NAME}:generateContent?key={_apiKey}";

                var response = await _httpClient.PostAsync(url, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[GEMINI ERROR - MODERATION] Status: {response.StatusCode} | Detalii: {errorBody}");
                    return true; // Fail open
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var jsonNode = JsonNode.Parse(responseString);
                var resultText = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString().Trim();

                if (resultText != null && resultText.Contains("\"is_safe\": false"))
                {
                    return false; // BLOCHEAZA
                }

                return true; // PERMITE
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GEMINI EXCEPTION]: {ex.Message}");
                return true;
            }
        }

        // === METODA 2: TRADUCERE (HOROSCOP) ===
        public async Task<string> TranslateToRomanian(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "Nu există text de tradus.";

            var prompt = $@"
Role: Professional Translator.
Task: Translate the following horoscope text into Romanian.
Tone: Mystical, friendly, and astrological.
Text: ""{text}""
Response: Only the translated text, nothing else.
";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{MODEL_NAME}:generateContent?key={_apiKey}";

                var response = await _httpClient.PostAsync(url, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var jsonNode = JsonNode.Parse(responseString);
                    var translatedText = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString().Trim();

                    return translatedText ?? text;
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[GEMINI ERROR - TRANSLATE] Status: {response.StatusCode} | Detalii: {errorBody}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TRANSLATION ERROR]: {ex.Message}");
            }

            return text; // Returnam originalul daca pica
        }
    }
}