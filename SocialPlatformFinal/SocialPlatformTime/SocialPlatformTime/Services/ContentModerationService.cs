using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SocialPlatformTime.Services
{
    // Clasa pentru rezultatul moderării conținutului
    public class ContentModerationResult
    {
        public bool IsAppropriate { get; set; } = true; // true = conținut adecvat, false = neadecvat
        public double Confidence { get; set; } = 0.0; // 0.0 - 1.0
        public bool Success { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public string? Reason { get; set; } // Motivul pentru care conținutul este neadecvat
    }

    // Interfața serviciului pentru dependency injection
    public interface IContentModerationService
    {
        Task<ContentModerationResult> ModerateContentAsync(string text);
    }

    public class GoogleContentModerationService : IContentModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GoogleContentModerationService> _logger;

        // URL-ul de bază pentru API-ul Google Generative AI
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";
        // Modelul folosit - gemini-2.5-flash-lite
        private const string ModelName = "gemini-2.5-flash-lite";

        public GoogleContentModerationService(IConfiguration configuration, ILogger<GoogleContentModerationService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["GoogleAI:ApiKey"]
                ?? throw new ArgumentNullException("GoogleAI:ApiKey nu este configurat în appsettings.json");
            _logger = logger;

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Verifică dacă un text conține limbaj nepotrivit (insulte, hate speech, etc.)
        /// </summary>
        /// <param name="text">Textul de verificat</param>
        /// <returns>Rezultatul moderării</returns>
        public async Task<ContentModerationResult> ModerateContentAsync(string text)
        {
            try
            {
                // Dacă textul este gol sau doar spații, considerăm că este adecvat
                if (string.IsNullOrWhiteSpace(text))
                {
                    return new ContentModerationResult
                    {
                        IsAppropriate = true,
                        Success = true,
                        Confidence = 1.0
                    };
                }

                // Construim prompt-ul pentru moderarea conținutului
                var prompt = $@"You are a content moderation assistant. Analyze the following text and determine if it contains inappropriate content such as:
- Insults or offensive language
- Hate speech or discriminatory language
- Threats or violent language
- Harassment or bullying
- Explicit sexual content
- Other inappropriate content

Respond ONLY with a JSON object in this exact format:
{{""isAppropriate"": true/false, ""confidence"": 0.0-1.0, ""reason"": ""brief explanation if inappropriate""}}

Rules:
- isAppropriate: true if content is appropriate, false if it contains inappropriate content
- confidence: a number between 0.0 and 1.0 indicating how confident you are
- reason: only provide if isAppropriate is false, briefly explain why (e.g., ""contains insults"", ""hate speech detected"")
- Do not include any other text, only the JSON object

Text to analyze: ""{text}""";

                // Construim request-ul pentru Google AI API
                var requestBody = new GoogleAiRequest
                {
                    Contents = new List<GoogleAiContent>
                    {
                        new GoogleAiContent
                        {
                            Parts = new List<GoogleAiPart>
                            {
                                new GoogleAiPart { Text = prompt }
                            }
                        }
                    },
                    GenerationConfig = new GoogleAiGenerationConfig
                    {
                        Temperature = 0.1,
                        MaxOutputTokens = 150
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var requestUrl = $"{BaseUrl}{ModelName}:generateContent?key={_apiKey}";

                _logger.LogInformation("Trimitem cererea de moderare conținut către Google AI API");

                // Trimitem request-ul către Google AI API
                var response = await _httpClient.PostAsync(requestUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Eroare Google AI API: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new ContentModerationResult
                    {
                        Success = false,
                        ErrorMessage = $"Eroare API: {response.StatusCode}",
                        // În caz de eroare, permitem conținutul pentru a nu bloca utilizatorii
                        IsAppropriate = true
                    };
                }

                // Parsăm răspunsul de la Google AI
                var googleResponse = JsonSerializer.Deserialize<GoogleAiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Extragem textul din răspuns
                var assistantMessage = googleResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
                if (string.IsNullOrEmpty(assistantMessage))
                {
                    return new ContentModerationResult
                    {
                        Success = false,
                        ErrorMessage = "Răspuns gol de la API",
                        IsAppropriate = true // Permitem conținutul în caz de eroare
                    };
                }

                _logger.LogInformation("Răspuns Google AI pentru moderare: {Response}", assistantMessage);

                // Curățăm răspunsul de eventuale caractere markdown
                var cleanedResponse = CleanJsonResponse(assistantMessage);

                // Parsăm JSON-ul din răspunsul asistentului
                var moderationData = JsonSerializer.Deserialize<ModerationResponse>(cleanedResponse);

                if (moderationData == null)
                {
                    return new ContentModerationResult
                    {
                        Success = false,
                        ErrorMessage = "Nu s-a putut parsa răspunsul de moderare",
                        IsAppropriate = true // Permitem conținutul în caz de eroare
                    };
                }

                // Validăm confidence score
                var confidence = Math.Clamp(moderationData.Confidence, 0.0, 1.0);

                return new ContentModerationResult
                {
                    IsAppropriate = moderationData.IsAppropriate,
                    Confidence = confidence,
                    Reason = moderationData.Reason,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la moderarea conținutului");
                return new ContentModerationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    // În caz de excepție, permitem conținutul pentru a nu bloca utilizatorii
                    IsAppropriate = true
                };
            }
        }

        /// <summary>
        /// Curăță răspunsul JSON de eventuale caractere markdown
        /// </summary>
        private string CleanJsonResponse(string response)
        {
            var cleaned = response.Trim();
            if (cleaned.StartsWith(""))
            {
                cleaned = cleaned.Substring(7);
            }
            else if (cleaned.StartsWith("```"))
            {
                cleaned = cleaned.Substring(3);
            }
            if (cleaned.EndsWith("```"))
            {
                cleaned = cleaned.Substring(0, cleaned.Length - 3);
            }
            return cleaned.Trim();
        }
    }

    /// <summary>
    /// Clasă intermediară pentru parsarea JSON-ului brut primit de la AI
    /// </summary>
    public class ModerationResponse
    {
        [JsonPropertyName("isAppropriate")]
        public bool IsAppropriate { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }
}