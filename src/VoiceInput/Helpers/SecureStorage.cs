using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace VoiceInput.Helpers;

public static class SecureStorage
{
    private const string RegistryKeyPath = @"Software\VoiceInputApp";
    private const string ApiKeyValName = "SecureApiKey";

    public static void SaveApiKey(string apiKey)
    {
        SaveSecret(ApiKeyValName, apiKey);
    }

    public static string LoadApiKey()
    {
        return LoadSecret(ApiKeyValName);
    }

    public static void SaveSecret(string valName, string secret)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(secret);
            byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            if (key != null)
            {
                key.SetValue(valName, encrypted, RegistryValueKind.Binary);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error encrypting {valName}: {ex.Message}");
        }
    }

    public static string LoadSecret(string valName)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key?.GetValue(valName) is byte[] encrypted)
            {
                byte[] data = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(data);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error decrypting {valName}: {ex.Message}");
        }
        return string.Empty;
    }
}
