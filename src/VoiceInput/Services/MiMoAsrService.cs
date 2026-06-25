using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VoiceInput.Services;

public class MiMoAsrService : IAsrService
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
        string provider = settings.AsrProvider;
        string baseUrl = settings.AsrBaseUrl.TrimEnd('/');
        string model = settings.AsrModelName;

        if (string.IsNullOrEmpty(apiKey) && provider != "custom")
        {
            throw new InvalidOperationException("API Key is missing. Please configure it in settings.");
        }

        if (provider == "openai" || (provider == "custom" && (baseUrl.Contains("audio") || model.Contains("whisper"))))
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/audio/transcriptions");
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            }

            var multipart = new MultipartFormDataContent();
            byte[] audioBytes = Convert.FromBase64String(base64Audio);
            var fileContent = new ByteArrayContent(audioBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
            multipart.Add(fileContent, "file", mimeType.Contains("wav") ? "audio.wav" : "audio.mp3");
            multipart.Add(new StringContent(model), "model");

            if (!string.IsNullOrEmpty(language) && language != "auto")
            {
                multipart.Add(new StringContent(language), "language");
            }

            request.Content = multipart;
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("text").GetString() ?? string.Empty;
        }
        else
        {
            string dataUrl = $"data:{mimeType};base64,{base64Audio}";
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
                asr_options = new { language = language }
            };

            string json = JsonSerializer.Serialize(requestBody);
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/chat/completions");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            ApplyAuth(request, apiKey, provider);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }
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
        string provider = settings.AsrProvider;
        string baseUrl = settings.AsrBaseUrl.TrimEnd('/');
        string model = settings.AsrModelName;

        if (string.IsNullOrEmpty(apiKey) && provider != "custom")
        {
            throw new InvalidOperationException("API Key is missing. Please configure it in settings.");
        }

        if (provider == "openai" || (provider == "custom" && (baseUrl.Contains("audio") || model.Contains("whisper"))))
        {
            string result = await RecognizeAsync(base64Audio, mimeType, language, customApiKey);
            if (!string.IsNullOrEmpty(result))
            {
                onResult(result);
            }
            return;
        }

        string dataUrl = $"data:{mimeType};base64,{base64Audio}";

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
        string requestUrl = $"{baseUrl}/chat/completions";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        ApplyAuth(request, apiKey, provider);

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
