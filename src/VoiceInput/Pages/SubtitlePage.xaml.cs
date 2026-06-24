using System;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Windows.ApplicationModel.DataTransfer;

namespace VoiceInput.Pages;

public partial class SubtitlePage : Page
{
    private readonly StringBuilder _allSubtitles = new();

    private Paragraph ContentParagraph
    {
        get
        {
            if (SubtitleBox.Blocks.Count == 0)
            {
                var p = new Paragraph { FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe UI Variable"), FontSize = 16, Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)) };
                SubtitleBox.Blocks.Add(p);
            }
            return (Paragraph)SubtitleBox.Blocks[0];
        }
    }

    public SubtitlePage()
    {
        this.InitializeComponent();
        
        string lang = Helpers.Settings.Instance.Current.Language;
        LanguageTxt.Text = lang == "auto" ? "自动检测" : (lang == "zh" ? "中文" : "英文");
    }

    public void AppendSubtitle(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        DispatcherQueue.TryEnqueue(() =>
        {
            if (_allSubtitles.Length == 0)
            {
                ContentParagraph.Inlines.Clear();
            }

            string timestamp = $"[{DateTime.Now:HH:mm:ss}] ";
            _allSubtitles.AppendLine($"{timestamp}{text}");

            var runTime = new Run { Text = timestamp, Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 212)) };
            var runTxt = new Run { Text = text + "\n", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)) };

            ContentParagraph.Inlines.Add(runTime);
            ContentParagraph.Inlines.Add(runTxt);

            ScrollContainer.ChangeView(null, ScrollContainer.ScrollableHeight, null);
        });
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        _allSubtitles.Clear();
        ContentParagraph.Inlines.Clear();
        ContentParagraph.Inlines.Add(new Run { Text = "字幕内容将在这里实时显示...", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 85, 85, 85)) });
    }

    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        if (_allSubtitles.Length > 0)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(_allSubtitles.ToString());
            Clipboard.SetContent(dataPackage);
        }
    }
}
