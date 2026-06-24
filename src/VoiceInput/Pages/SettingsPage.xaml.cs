using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace VoiceInput.Pages;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = Helpers.Settings.Instance.Current;
        ApiKeyBox.Password = settings.ApiKey;
        
        if (settings.Language == "auto") LangAutoBtn.IsChecked = true;
        else if (settings.Language == "zh") LangZhBtn.IsChecked = true;
        else if (settings.Language == "en") LangEnBtn.IsChecked = true;

        foreach (ComboBoxItem item in HotkeyCombo.Items)
        {
            if (item.Tag?.ToString() == settings.Hotkey)
            {
                HotkeyCombo.SelectedItem = item;
                break;
            }
        }

        LlmCorrectionSwitch.IsOn = settings.LlmCorrection;

        if (settings.InterfaceLanguage == "zh-CN") UiZhBtn.IsChecked = true;
        else if (settings.InterfaceLanguage == "en-US") UiEnBtn.IsChecked = true;

        HudOffsetSlider.Value = settings.HudOffset;
    }

    private async void OnTestConnectionClick(object sender, RoutedEventArgs e)
    {
        string apiKey = ApiKeyBox.Password;
        if (string.IsNullOrEmpty(apiKey))
        {
            TestResultTxt.Text = "✗ 请输入 API Key";
            TestResultTxt.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 216, 59, 1));
            return;
        }

        TestResultTxt.Text = "正在连接测试...";
        TestResultTxt.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 153, 153, 153));

        try
        {
            using var client = new HttpClient();
            var reqBody = new
            {
                model = "mimo-v2.5-asr",
                messages = new[] { new { role = "user", content = "ping" } }
            };
            var content = new StringContent(JsonSerializer.Serialize(reqBody), System.Text.Encoding.UTF8, "application/json");
            content.Headers.Add("api-key", apiKey);

            var resp = await client.PostAsync("https://api.xiaomimimo.com/v1/chat/completions", content);
            if (resp.IsSuccessStatusCode)
            {
                TestResultTxt.Text = "✓ 连接成功！";
                TestResultTxt.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 124, 65));
            }
            else
            {
                TestResultTxt.Text = $"✗ 连接失败: {resp.StatusCode}";
                TestResultTxt.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 216, 59, 1));
            }
        }
        catch (Exception ex)
        {
            TestResultTxt.Text = $"✗ 连接错误: {ex.Message}";
            TestResultTxt.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 216, 59, 1));
        }
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var settings = Helpers.Settings.Instance.Current;
        settings.ApiKey = ApiKeyBox.Password;

        if (LangAutoBtn.IsChecked == true) settings.Language = "auto";
        else if (LangZhBtn.IsChecked == true) settings.Language = "zh";
        else if (LangEnBtn.IsChecked == true) settings.Language = "en";

        if (HotkeyCombo.SelectedItem is ComboBoxItem item)
        {
            settings.Hotkey = item.Tag?.ToString() ?? "RightAlt";
        }

        settings.LlmCorrection = LlmCorrectionSwitch.IsOn;

        if (UiZhBtn.IsChecked == true) settings.InterfaceLanguage = "zh-CN";
        else if (UiEnBtn.IsChecked == true) settings.InterfaceLanguage = "en-US";

        settings.HudOffset = (int)HudOffsetSlider.Value;

        Helpers.Settings.Instance.Save();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        LoadSettings();
    }
}
