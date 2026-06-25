using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace VoiceInput.AppWindows;

public partial class HudWindow : Window
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_NOACTIVATE = 0x08000000;

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;

    private DispatcherTimer? _waveTimer;
    private readonly Random _rand = new();

    public HudWindow()
    {
        this.InitializeComponent();

        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TOOLWINDOW | WS_EX_TOPMOST | WS_EX_NOACTIVATE);

        SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

        CenterOnScreen(hwnd);
        StartPlaceholderAnimation();
    }

    private void CenterOnScreen(IntPtr hwnd)
    {
        int screenWidth = 1920; 
        int screenHeight = 1080;

        int x = (screenWidth - 320) / 2;
        int y = screenHeight - 40 - Helpers.Settings.Instance.Current.HudOffset;

        SetWindowPos(hwnd, HWND_TOPMOST, x, y, 320, 40, SWP_NOACTIVATE);
    }

    private void StartPlaceholderAnimation()
    {
        _waveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _waveTimer.Tick += (s, e) =>
        {
            Bar1.Height = _rand.Next(4, 12);
            Bar2.Height = _rand.Next(6, 16);
            Bar3.Height = _rand.Next(10, 20);
            Bar4.Height = _rand.Next(6, 14);
            Bar5.Height = _rand.Next(4, 12);
        };
        _waveTimer.Start();
    }

    public void StartRecording()
    {
        TranscribeTxt.Text = "请说话...";
        TranscribeTxt.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
    }

    public void ShowTranscribing()
    {
        TranscribeTxt.Text = "正在转录...";
    }

    public void ShowResult(string text)
    {
        _waveTimer?.Stop();
        
        Bar1.Height = 4; Bar2.Height = 8; Bar3.Height = 14; Bar4.Height = 6; Bar5.Height = 10;

        TranscribeTxt.Text = string.IsNullOrEmpty(text) ? "未识别到语音" : text;
        
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            this.Close();
        };
        timer.Start();
    }

    public void ShowError(string message)
    {
        _waveTimer?.Stop();
        TranscribeTxt.Text = $"错误: {message}";
        TranscribeTxt.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 216, 59, 1));
        
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            this.Close();
        };
        timer.Start();
    }
}
