using System;

namespace VoiceInput.Models;

public class AppSettings
{
    // ASR Configuration
    public string AsrProvider { get; set; } = "mimo"; // "mimo", "openai", "custom"
    public string AsrBaseUrl { get; set; } = "https://api.xiaomimimo.com/v1";
    public string AsrApiKey { get; set; } = string.Empty;
    public string AsrModelName { get; set; } = "mimo-v2.5-asr";

    // Backward Compatibility Mappings
    public string ApiBaseUrl { get => AsrBaseUrl; set => AsrBaseUrl = value; }
    public string ApiKey { get => AsrApiKey; set => AsrApiKey = value; }
    public string ModelName { get => AsrModelName; set => AsrModelName = value; }

    // LLM Configuration
    public string LlmProvider { get; set; } = "mimo"; // "mimo", "openai", "gemini", "custom"
    public string LlmBaseUrl { get; set; } = "https://api.xiaomimimo.com/v1";
    public string LlmApiKey { get; set; } = string.Empty;
    public string LlmModelName { get; set; } = "gpt-3.5-turbo";
    public string LlmCorrectionMode { get; set; } = "standard"; // "standard", "professional", "academic", "colloquial"

    // General Configuration
    public string Language { get; set; } = "auto"; // auto, zh, en
    public string Hotkey { get; set; } = "RightAlt";
    public bool LlmCorrection { get; set; } = true;
    public string InterfaceLanguage { get; set; } = "zh-CN"; // zh-CN, en-US
    public int HudOffset { get; set; } = 32; // Default 32px from bottom of the screen
}
