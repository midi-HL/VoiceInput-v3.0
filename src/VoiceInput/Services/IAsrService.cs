using System;
using System.Threading.Tasks;

namespace VoiceInput.Services;

public interface IAsrService
{
    Task<string> RecognizeAsync(string base64Audio, string mimeType = "audio/wav", string language = "auto", string? customApiKey = null);
    Task StreamRecognizeAsync(string base64Audio, Action<string> onResult, string mimeType = "audio/wav", string language = "auto", string? customApiKey = null);
}
