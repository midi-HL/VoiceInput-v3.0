using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace VoiceInput.Pages;

public partial class LyricsPage : Page
{
    private string? _selectedFilePath;
    private DispatcherTimer? _timer;
    private Stopwatch? _stopwatch;

    public LyricsPage()
    {
        this.InitializeComponent();
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        UploadBorder.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 212));
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        UploadBorder.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 85, 85, 85));
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        UploadBorder.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 85, 85, 85));
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count > 0 && items[0] is StorageFile file)
            {
                ProcessSelectedFile(file.Path);
            }
        }
    }

    private async void OnUploadClick(object sender, PointerRoutedEventArgs e)
    {
        if (_selectedFilePath != null) return;

        var picker = new FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.As<App>()._mainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        picker.ViewMode = PickerViewMode.List;
        picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
        picker.FileTypeFilter.Add(".wav");
        picker.FileTypeFilter.Add(".mp3");

        StorageFile file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            ProcessSelectedFile(file.Path);
        }
    }

    private void ProcessSelectedFile(string path)
    {
        string ext = Path.GetExtension(path).ToLower();
        if (ext != ".wav" && ext != ".mp3")
        {
            ShowErrorMessage("不支持的格式 请上传 WAV 或 MP3");
            return;
        }

        var fileInfo = new FileInfo(path);
        if (fileInfo.Length > 10 * 1024 * 1024)
        {
            ShowErrorMessage("文件超过 10MB 限制 请压缩后重试");
            return;
        }

        _selectedFilePath = path;
        FileNameTxt.Text = Path.GetFileName(path);
        FileSizeTxt.Text = $"{(fileInfo.Length / 1024.0 / 1024.0):F2} MB";

        EmptyUploadState.Visibility = Visibility.Collapsed;
        FileSelectedState.Visibility = Visibility.Visible;
        StartBtn.IsEnabled = true;
    }

    private void OnDeleteFileClick(object sender, RoutedEventArgs e)
    {
        _selectedFilePath = null;
        FileSelectedState.Visibility = Visibility.Collapsed;
        EmptyUploadState.Visibility = Visibility.Visible;
        
        StartBtn.IsEnabled = false;
        ExportBtn.IsEnabled = false;
        CopyBtn.IsEnabled = false;

        LyricsTxt.Visibility = Visibility.Collapsed;
        IdleStateTxt.Visibility = Visibility.Visible;
    }

    private async void OnStartRecognizeClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedFilePath)) return;

        IdleStateTxt.Visibility = Visibility.Collapsed;
        LyricsTxt.Visibility = Visibility.Collapsed;
        LoadingState.Visibility = Visibility.Visible;
        StartBtn.IsEnabled = false;

        _stopwatch = Stopwatch.StartNew();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, ev) =>
        {
            TimerTxt.Text = $"已耗时: {_stopwatch.Elapsed.Seconds}s";
        };
        _timer.Start();

        try
        {
            var lyricsService = new LyricsRecognizer();
            string lyrics = await Task.Run(() => lyricsService.RecognizeLyricsAsync(_selectedFilePath));

            _timer.Stop();
            _stopwatch.Stop();

            LoadingState.Visibility = Visibility.Collapsed;
            LyricsTxt.Text = lyrics;
            LyricsTxt.Visibility = Visibility.Visible;

            ExportBtn.IsEnabled = true;
            CopyBtn.IsEnabled = true;
        }
        catch (Exception ex)
        {
            _timer.Stop();
            _stopwatch.Stop();
            LoadingState.Visibility = Visibility.Collapsed;
            ShowErrorMessage(ex.Message);
        }
        finally
        {
            StartBtn.IsEnabled = true;
        }
    }

    private async void OnExportClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileSavePicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.As<App>()._mainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeChoices.Add("Lyric File", new System.Collections.Generic.List<string>() { ".lrc" });
        picker.SuggestedFileName = Path.GetFileNameWithoutExtension(_selectedFilePath) ?? "lyrics";

        StorageFile file = await picker.PickSaveFileAsync();
        if (file != null)
        {
            await FileIO.WriteTextAsync(file, LyricsTxt.Text);
        }
    }

    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(LyricsTxt.Text);
        Clipboard.SetContent(dataPackage);
    }

    private void ShowErrorMessage(string message)
    {
        IdleStateTxt.Text = $"错误: {message}";
        IdleStateTxt.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 216, 59, 1));
        IdleStateTxt.Visibility = Visibility.Visible;
    }
}
