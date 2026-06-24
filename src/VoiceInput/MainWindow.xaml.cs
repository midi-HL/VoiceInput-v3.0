using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace VoiceInput;

public partial class MainWindow : Window
{
    private TrayIcon? _trayIcon;
    private WindowsSystemDispatcherQueueHelper? _wsdqHelper;
    private MicaController? _micaController;
    private SystemBackdropConfiguration? _configurationSource;

    public MainWindow()
    {
        this.InitializeComponent();
        
        this.Title = "语音输入";
        this.ExtendsContentIntoTitleBar = true;
        this.SetTitleBar(null);
        
        TrySetSystemBackdrop();

        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        _trayIcon = new TrayIcon(hwnd, 1, "语音输入法 (运行中)");

        ContentFrame.Navigate(typeof(Pages.HomePage));
        UpdateActiveNavState("Home");
    }

    private void OnNavClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string pageTag)
        {
            switch (pageTag)
            {
                case "Home":
                    ContentFrame.Navigate(typeof(Pages.HomePage));
                    break;
                case "Lyrics":
                    ContentFrame.Navigate(typeof(Pages.LyricsPage));
                    break;
                case "Subtitle":
                    ContentFrame.Navigate(typeof(Pages.SubtitlePage));
                    break;
                case "Settings":
                    ContentFrame.Navigate(typeof(Pages.SettingsPage));
                    break;
            }
            UpdateActiveNavState(pageTag);
        }
    }

    private void UpdateActiveNavState(string selectedTag)
    {
        foreach (var child in NavList.Children)
        {
            if (child is Button btn && btn.Tag is string tag)
            {
                bool isSelected = tag == selectedTag;
                if (btn.Content is StackPanel sp)
                {
                    foreach (var spChild in sp.Children)
                    {
                        if (spChild is FontIcon fi)
                        {
                            fi.Foreground = isSelected ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 212)) : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 153, 153, 153));
                        }
                        else if (spChild is TextBlock tb)
                        {
                            tb.Foreground = isSelected ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)) : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 153, 153, 153));
                        }
                    }
                }
                btn.Background = isSelected ? new SolidColorBrush(Windows.UI.Color.FromArgb(25, 0, 120, 212)) : new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
            }
        }

        bool isSettingsSelected = selectedTag == "Settings";
        SettingsBtn.Background = isSettingsSelected ? new SolidColorBrush(Windows.UI.Color.FromArgb(25, 0, 120, 212)) : new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
        if (SettingsBtn.Content is StackPanel ssp)
        {
            foreach (var sspChild in ssp.Children)
            {
                if (sspChild is FontIcon sfi)
                    sfi.Foreground = isSettingsSelected ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 212)) : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 153, 153, 153));
                else if (sspChild is TextBlock stb)
                    stb.Foreground = isSettingsSelected ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)) : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 153, 153, 153));
            }
        }
    }

    private void OnNavigationFailed(object sender, Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void TrySetSystemBackdrop()
    {
        if (MicaController.IsSupported())
        {
            _wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            _wsdqHelper.EnsureWindowsSystemDispatcherQueue();

            _configurationSource = new SystemBackdropConfiguration();
            this.Activated += Window_Activated;
            this.Closed += Window_Closed;

            _configurationSource.IsInputActive = true;
            _micaController = new MicaController();
            _micaController.AddSystemBackdropTarget(WinRT.CastExtensions.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>(this));
            _micaController.SetSystemBackdropConfiguration(_configurationSource);
        }
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (_configurationSource != null)
        {
            _configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        _trayIcon?.Dispose();
        _trayIcon = null;

        if (_micaController != null)
        {
            _micaController.Dispose();
            _micaController = null;
        }
        this.Activated -= Window_Activated;
        _configurationSource = null;
    }
}

internal class WindowsSystemDispatcherQueueHelper
{
    [StructLayout(LayoutKind.Sequential)]
    struct DispatcherQueueOptions
    {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }

    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out] ref IntPtr dispatcherQueueController);

    private IntPtr _dispatcherQueueController = IntPtr.Zero;

    public void EnsureWindowsSystemDispatcherQueue()
    {
        if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
        {
            return;
        }

        if (_dispatcherQueueController == IntPtr.Zero)
        {
            DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf<DispatcherQueueOptions>();
            options.threadType = 2;    // DQTYPE_THREAD_CURRENT
            options.apartmentType = 2; // DQTAT_COM_STA

            CreateDispatcherQueueController(options, ref _dispatcherQueueController);
        }
    }
}
