using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VoiceInput.Services;

public class LlmRefiner : ILlmService
{
    private readonly HttpClient _httpClient;
    private const string PROMPT_STANDARD = @"你是语音转录后处理助手。
你的唯一任务是修复明显的语音识别错误。
绝对禁止改写、润色、补充或删除任何看起来正确的内容。
如果输入内容看起来已经正确 必须原样返回。";

    private const string PROMPT_PROFESSIONAL = @"你是专业的商务与办公文字排版和修正助理。
请将输入的语音转录文本修正为规范、流畅的书面办公用语。
进行错别字修正、标点补全，并适当精简重复词汇，使语言更得体、高效。";

    private const string PROMPT_ACADEMIC = @"你是严谨的学术和技术报告文字修正助理。
请将输入文本中的语音识别错误修正，并将语气和措辞调整为学术、客观、逻辑严密的正式文体。
使用规范的专业术语，去除口语化和冗余表达。";

    private const string PROMPT_COLLOQUIAL = @"你是日常口语记录助手。
请仅修正由于同音字或发音不清导致的明显错别字。
保留原说话者的语气、叹词、语气词和所有原本的口头表达习惯，不要进行任何书面语包装。";

    public LlmRefiner()
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

    private string GetSystemPrompt(string mode)
    {
        return mode switch
        {
            "professional" => PROMPT_PROFESSIONAL,
            "academic" => PROMPT_ACADEMIC,
            "colloquial" => PROMPT_COLLOQUIAL,
            _ => PROMPT_STANDARD
        };
    }

    public async Task<string> RefineTextAsync(string rawText, string? customApiKey = null)
    {
        var settings = Helpers.Settings.Instance.Current;
        string apiKey = !string.IsNullOrEmpty(customApiKey) ? customApiKey : settings.LlmApiKey;
        if (string.IsNullOrEmpty(apiKey))
        {
            return rawText;
        }

        try
        {
            string systemPrompt = GetSystemPrompt(settings.LlmCorrectionMode);
            var requestBody = new
            {
                model = settings.LlmModelName,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = rawText }
                },
                temperature = 0.2
            };

            string json = JsonSerializer.Serialize(requestBody);
            string baseUrl = settings.LlmBaseUrl.TrimEnd('/');
            string requestUrl;
            if (settings.LlmProvider == "gemini" && baseUrl.Contains("generativelanguage.googleapis.com"))
            {
                requestUrl = $"{baseUrl}/openai/chat/completions";
            }
            else
            {
                requestUrl = $"{baseUrl}/chat/completions";
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            ApplyAuth(request, apiKey, settings.LlmProvider);

            var response = await _httpClient.SendAsync(request);

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
