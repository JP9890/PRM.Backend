using System.Threading.Tasks;

namespace PRMTool.Application.Interfaces
{
    public interface ILLMProvider
    {
        Task<string> GenerateTextAsync(string prompt, string apiKey);
    }
}
