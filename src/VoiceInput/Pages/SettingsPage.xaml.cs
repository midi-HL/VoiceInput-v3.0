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

    private void SetComboBoxByTag(ComboBox comboBox, string tagValue)
    {
        foreach (ComboBoxItem item in comboBox.Items)
        {
            if (item.Tag?.ToString() == tagValue)
            {
                comboBox.SelectedItem = item;
                break;
            }
        }
    }

    private void LoadSettings()
    {
        var settings = Helpers.Settings.Instance.Current;
        
        // Load ASR settings
        SetComboBoxByTag(AsrProviderCombo, settings.AsrProvider);
        AsrBaseUrlBox.Text = settings.AsrBaseUrl;
        AsrApiKeyBox.Password = settings.AsrApiKey;
        AsrModelNameBox.Text = settings.AsrModelName;

        // Load language settings
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

        // Load LLM settings
        LlmCorrectionSwitch.IsOn = settings.LlmCorrection;
        LlmDetailsPanel.Visibility = settings.LlmCorrection ? Visibility.Visible : Visibility.Collapsed;

        SetComboBoxByTag(LlmProviderCombo, settings.LlmProvider);
        LlmBaseUrlBox.Text = settings.LlmBaseUrl;
        LlmApiKeyBox.Password = settings.LlmApiKey;
        LlmModelNameBox.Text = settings.LlmModelName;
        SetComboBoxByTag(LlmModeCombo, settings.LlmCorrectionMode);

        // Load display/UI settings
        if (settings.InterfaceLanguage == "zh-CN") UiZhBtn.IsChecked = true;
        else if (settings.InterfaceLanguage == "en-US") UiEnBtn.IsChecked = true;

        HudOffsetSlider.Value = settings.HudOffset;
    }

    private void OnAsrProviderChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AsrBaseUrlBox == null || AsrModelNameBox == null) return;
        
        if (AsrProviderCombo.SelectedItem is ComboBoxItem item && item.Tag is string provider)
        {
            if (provider == "mimo")
            {
                AsrBaseUrlBox.Text = "https://api.xiaomimimo.com/v1";
                AsrModelNameBox.Text = "mimo-v2.5-asr";
            }
            else if (provider == "openai")
            {
                AsrBaseUrlBox.Text = "https://api.openai.com/v1";
                AsrModelNameBox.Text = "whisper-1";
            }
        }
    }

    private void OnLlmProviderChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LlmBaseUrlBox == null || LlmModelNameBox == null) return;

        if (LlmProviderCombo.SelectedItem is ComboBoxItem item && item.Tag is string provider)
        {
            if (provider == "mimo")
            {
                LlmBaseUrlBox.Text = "https://api.xiaomimimo.com/v1";
                LlmModelNameBox.Text = "gpt-3.5-turbo";
            }
            else if (provider == "openai")
            {
                LlmBaseUrlBox.Text = "https://api.openai.com/v1";
                LlmModelNameBox.Text = "gpt-4o-mini";
            }
            else if (provider == "gemini")
            {
                LlmBaseUrlBox.Text = "https://generativelanguage.googleapis.com/v1beta";
                LlmModelNameBox.Text = "gemini-2.5-flash";
            }
        }
    }

    private void OnLlmCorrectionToggled(object sender, RoutedEventArgs e)
    {
        if (LlmDetailsPanel != null)
        {
            LlmDetailsPanel.Visibility = LlmCorrectionSwitch.IsOn ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private async void OnTestConnectionClick(object sender, RoutedEventArgs e)
    {
        string provider = (AsrProviderCombo.SelectedItem is ComboBoxItem asrItem) ? (asrItem.Tag?.ToString() ?? "mimo") : "mimo";
        string baseUrl = AsrBaseUrlBox.Text;
        string apiKey = AsrApiKeyBox.Password;
        string modelName = AsrModelNameBox.Text;

        if (string.IsNullOrEmpty(apiKey))
        {
            TestResultTxt.Text = "✗ 请输入 ASR API Key";
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
                model = modelName,
                messages = new[] { new { role = "user", content = "ping" } }
            };
            var content = new StringContent(JsonSerializer.Serialize(reqBody), System.Text.Encoding.UTF8, "application/json");
            
            if (provider == "mimo")
            {
                content.Headers.Add("api-key", apiKey);
            }
            else
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            }

            string requestUrl = $"{baseUrl.TrimEnd('/')}/chat/completions";
            var resp = await client.PostAsync(requestUrl, content);
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

        // Save ASR settings
        if (AsrProviderCombo.SelectedItem is ComboBoxItem asrItem)
        {
            settings.AsrProvider = asrItem.Tag?.ToString() ?? "mimo";
        }
        settings.AsrBaseUrl = AsrBaseUrlBox.Text;
        settings.AsrApiKey = AsrApiKeyBox.Password;
        settings.AsrModelName = AsrModelNameBox.Text;

        // Save recognition settings
        if (LangAutoBtn.IsChecked == true) settings.Language = "auto";
        else if (LangZhBtn.IsChecked == true) settings.Language = "zh";
        else if (LangEnBtn.IsChecked == true) settings.Language = "en";

        if (HotkeyCombo.SelectedItem is ComboBoxItem item)
        {
            settings.Hotkey = item.Tag?.ToString() ?? "RightAlt";
        }

        // Save LLM settings
        settings.LlmCorrection = LlmCorrectionSwitch.IsOn;
        if (LlmProviderCombo.SelectedItem is ComboBoxItem llmItem)
        {
            settings.LlmProvider = llmItem.Tag?.ToString() ?? "mimo";
        }
        settings.LlmBaseUrl = LlmBaseUrlBox.Text;
        settings.LlmApiKey = LlmApiKeyBox.Password;
        settings.LlmModelName = LlmModelNameBox.Text;
        if (LlmModeCombo.SelectedItem is ComboBoxItem modeItem)
        {
            settings.LlmCorrectionMode = modeItem.Tag?.ToString() ?? "standard";
        }

        // Save display/UI settings
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
