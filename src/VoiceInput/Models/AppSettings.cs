using System;

namespace VoiceInput.Models;

public class AppSettings
{
    public string ApiBaseUrl { get; set; } = "https://api.xiaomimimo.com/v1";
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = "mimo-v2.5-asr";
    public string Language { get; set; } = "auto"; // auto, zh, en
    public string Hotkey { get; set; } = "RightAlt";
    public bool LlmCorrection { get; set; } = true;
    public string InterfaceLanguage { get; set; } = "zh-CN"; // zh-CN, en-US
    public int HudOffset { get; set; } = 32; // Default 32px from bottom of the screen
}
