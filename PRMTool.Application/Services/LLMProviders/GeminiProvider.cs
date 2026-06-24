using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PRMTool.Application.Interfaces;

namespace PRMTool.Application.Services.LLMProviders
{
    public class GeminiProvider : ILLMProvider
    {
        private readonly HttpClient _httpClient;

        public GeminiProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateTextAsync(string prompt, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("Gemini API key is required.");
            }

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var jsonPayload = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";
            var response = await _httpClient.PostAsync(url, httpContent);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
                    
                return text ?? string.Empty;
            }

            throw new Exception($"Gemini API failed with status code {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }
    }
}
