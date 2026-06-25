using System.Threading.Tasks;

namespace VoiceInput.Services;

public interface ILlmService
{
    Task<string> RefineTextAsync(string rawText, string? customApiKey = null);
}
