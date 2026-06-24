using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VoiceInput.Services;

public class LlmRefiner
{
    private readonly HttpClient _httpClient;
    private const string LLM_REFINE_PROMPT = @"你是语音转录后处理助手。
你的唯一任务是修复明显的语音识别错误。
绝对禁止改写、润色、补充或删除任何看起来正确的内容。
如果输入内容看起来已经正确 必须原样返回。";

    public LlmRefiner()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> RefineTextAsync(string rawText, string? customApiKey = null)
    {
        string apiKey = customApiKey ?? Helpers.SecureStorage.LoadApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            return rawText;
        }

        try
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = LLM_REFINE_PROMPT },
                    new { role = "user", content = rawText }
                },
                temperature = 0.2
            };

            string json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("api-key", apiKey);

            var response = await _httpClient.PostAsync(
                "https://api.xiaomimimo.com/v1/chat/completions", 
                content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                string refined = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? rawText;
                
                return refined.Trim();
            }
        }
        catch
        {
            // Ignore and fall back to raw text
        }

        return rawText;
    }
}
