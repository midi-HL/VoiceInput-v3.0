using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VoiceInput.Services;

public class MiMoAsrService
{
    private readonly HttpClient _httpClient;

    public MiMoAsrService()
    {
        _httpClient = new HttpClient();
    }

    private void ApplyAuth(HttpRequestMessage request, string apiKey, string provider)
    {
        if (provider == "mimo")
        {
            request.Headers.Add("api-key", apiKey);
        }
        else
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        }
    }

    public async Task<string> RecognizeAsync(string base64Audio, string mimeType = "audio/wav", string language = "auto", string? customApiKey = null)
    {
        var settings = Helpers.Settings.Instance.Current;
        string apiKey = !string.IsNullOrEmpty(customApiKey) ? customApiKey : settings.AsrApiKey;
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("API Key is missing. Please configure it in settings.");
        }

        string dataUrl = $"data:{mimeType};base64,{base64Audio}";
        string model = settings.AsrModelName;

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "input_audio",
                            input_audio = new
                            {
                                data = dataUrl
                            }
                        }
                    }
                }
            },
            asr_options = new
            {
                language = language
            }
        };

        string json = JsonSerializer.Serialize(requestBody);
        string baseUrl = settings.AsrBaseUrl.TrimEnd('/');
        string requestUrl = $"{baseUrl}/chat/completions";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        ApplyAuth(request, apiKey, settings.AsrProvider);

        var response = await _httpClient.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        string responseJson = await response.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(responseJson);
        string result = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;

        return result;
    }

    public async Task StreamRecognizeAsync(
        string base64Audio, 
        Action<string> onResult,
        string mimeType = "audio/wav",
        string language = "auto",
        string? customApiKey = null)
    {
        var settings = Helpers.Settings.Instance.Current;
        string apiKey = !string.IsNullOrEmpty(customApiKey) ? customApiKey : settings.AsrApiKey;
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("API Key is missing. Please configure it in settings.");
        }

        string dataUrl = $"data:{mimeType};base64,{base64Audio}";
        string model = settings.AsrModelName;

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "input_audio",
                            input_audio = new { data = dataUrl }
                        }
                    }
                }
            },
            asr_options = new { language = language },
            stream = true
        };

        string json = JsonSerializer.Serialize(requestBody);
        string baseUrl = settings.AsrBaseUrl.TrimEnd('/');
        string requestUrl = $"{baseUrl}/chat/completions";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        ApplyAuth(request, apiKey, settings.AsrProvider);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            string? line = await reader.ReadLineAsync();
            if (line?.StartsWith("data: ") == true)
            {
                string data = line.Substring(6);
                if (data == "[DONE]") break;

                try
                {
                    using var doc = JsonDocument.Parse(data);
                    string? content = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("delta")
                        .GetProperty("content")
                        .GetString();
                    
                    if (!string.IsNullOrEmpty(content))
                    {
                        onResult(content);
                    }
                }
                catch
                {
                    // Ignore formatting chunks
                }
            }
        }
    }
}
