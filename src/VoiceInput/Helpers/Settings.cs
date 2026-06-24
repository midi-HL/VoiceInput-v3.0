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

        _current.ApiKey = SecureStorage.LoadApiKey();
    }

    public void Save()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
            if (key != null)
            {
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

        SecureStorage.SaveApiKey(_current.ApiKey);
    }
}
