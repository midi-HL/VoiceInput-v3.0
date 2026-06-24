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
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(apiKey);
            byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            if (key != null)
            {
                key.SetValue(ApiKeyValName, encrypted, RegistryValueKind.Binary);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error encrypting API Key: {ex.Message}");
        }
    }

    public static string LoadApiKey()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key?.GetValue(ApiKeyValName) is byte[] encrypted)
            {
                byte[] data = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(data);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error decrypting API Key: {ex.Message}");
        }
        return string.Empty;
    }
}
