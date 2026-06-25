using System;
using Microsoft.Win32;
using VoiceInput.Models;

namespace VoiceInput.Helpers;

public class Settings
{
    private static readonly Lazy<Settings> _instance = new(() => new Settings());
    public static Settings Instance => _instance.Value;

    private readonly AppSettings _current;

    private const string RegistryPath = @"Software\VoiceInputApp\Preferences";

    private Settings()
    {
        _current = new AppSettings();
        Load();
    }

    public AppSettings Current => _current;

    public void Load()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
            if (key != null)
            {
                _current.AsrProvider = key.GetValue("AsrProvider", "mimo") as string ?? "mimo";
                _current.AsrBaseUrl = key.GetValue("AsrBaseUrl", "https://api.xiaomimimo.com/v1") as string ?? "https://api.xiaomimimo.com/v1";
                _current.AsrModelName = key.GetValue("AsrModelName", "mimo-v2.5-asr") as string ?? "mimo-v2.5-asr";

                _current.LlmProvider = key.GetValue("LlmProvider", "mimo") as string ?? "mimo";
                _current.LlmBaseUrl = key.GetValue("LlmBaseUrl", "https://api.xiaomimimo.com/v1") as string ?? "https://api.xiaomimimo.com/v1";
                _current.LlmModelName = key.GetValue("LlmModelName", "gpt-3.5-turbo") as string ?? "gpt-3.5-turbo";
                _current.LlmCorrectionMode = key.GetValue("LlmCorrectionMode", "standard") as string ?? "standard";

                _current.Language = key.GetValue("Language", "auto") as string ?? "auto";
                _current.Hotkey = key.GetValue("Hotkey", "RightAlt") as string ?? "RightAlt";
                _current.LlmCorrection = Convert.ToBoolean(key.GetValue("LlmCorrection", true));
                _current.InterfaceLanguage = key.GetValue("InterfaceLanguage", "zh-CN") as string ?? "zh-CN";
                _current.HudOffset = Convert.ToInt32(key.GetValue("HudOffset", 32));
            }
        }
        catch (Exception)
        {
            // Default settings
        }

        _current.AsrApiKey = SecureStorage.LoadSecret("SecureAsrApiKey");
        if (string.IsNullOrEmpty(_current.AsrApiKey))
        {
            // Fallback to legacy key
            _current.AsrApiKey = SecureStorage.LoadApiKey();
        }

        _current.LlmApiKey = SecureStorage.LoadSecret("SecureLlmApiKey");
        if (string.IsNullOrEmpty(_current.LlmApiKey))
        {
            _current.LlmApiKey = _current.AsrApiKey; // Default fallback to ASR key if empty
        }
    }

    public void Save()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
            if (key != null)
            {
                key.SetValue("AsrProvider", _current.AsrProvider);
                key.SetValue("AsrBaseUrl", _current.AsrBaseUrl);
                key.SetValue("AsrModelName", _current.AsrModelName);

                key.SetValue("LlmProvider", _current.LlmProvider);
                key.SetValue("LlmBaseUrl", _current.LlmBaseUrl);
                key.SetValue("LlmModelName", _current.LlmModelName);
                key.SetValue("LlmCorrectionMode", _current.LlmCorrectionMode);

                key.SetValue("Language", _current.Language);
                key.SetValue("Hotkey", _current.Hotkey);
                key.SetValue("LlmCorrection", _current.LlmCorrection);
                key.SetValue("InterfaceLanguage", _current.InterfaceLanguage);
                key.SetValue("HudOffset", _current.HudOffset);
            }
        }
        catch (Exception)
        {
            // Ignore
        }

        SecureStorage.SaveSecret("SecureAsrApiKey", _current.AsrApiKey);
        SecureStorage.SaveSecret("SecureLlmApiKey", _current.LlmApiKey);
        // Also sync legacy key for other apps/compatibility
        SecureStorage.SaveApiKey(_current.AsrApiKey);
    }
}
