using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PRMTool.Application.Interfaces;

namespace PRMTool.Application.Services.LLMProviders
{
    public class GemmaProvider : ILLMProvider
    {
        private readonly HttpClient _httpClient;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public GemmaProvider(HttpClient httpClient, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GenerateTextAsync(string prompt, string apiKey)
        {
            var requestBody = new
            {
                model = "gemma3:12b-it-q8_0",
                prompt = prompt,
                stream = false
            };

            var jsonPayload = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var url = _configuration["GemmaApiUrl"];
            if (string.IsNullOrWhiteSpace(url))
            {
                url = "http://164.52.211.238/api/generate"; // Default fallback
            }
            
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = httpContent
            };
            
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                // The custom Gemma endpoint expects the key in the "apikey" header
                request.Headers.Add("apikey", apiKey);
            }

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                
                // Assumes Ollama-style response format
                if (doc.RootElement.TryGetProperty("response", out var respElement))
                {
                    return respElement.GetString() ?? string.Empty;
                }
                
                // Fallback: just return the raw string if we can't parse it predictably
                return responseJson;
            }

            throw new Exception($"Gemma API failed with status code {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }
    }
}
