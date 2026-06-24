using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PRMTool.Application.Interfaces;

namespace PRMTool.Application.Services.LLMProviders
{
    public class GroqProvider : ILLMProvider
    {
        private readonly HttpClient _httpClient;

        public GroqProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateTextAsync(string prompt, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("Groq API key is required.");
            }

            var requestBody = new
            {
                model = "llama3-8b-8192", // Default Groq model, can be made configurable later
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var jsonPayload = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
            {
                Content = httpContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                
                var text = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
                    
                return text ?? string.Empty;
            }

            throw new Exception($"Groq API failed with status code {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }
    }
}
