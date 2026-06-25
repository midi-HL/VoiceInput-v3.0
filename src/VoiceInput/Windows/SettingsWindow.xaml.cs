using Microsoft.UI.Xaml;

namespace VoiceInput.Windows;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        this.InitializeComponent();
        
        // Set window size programmatically
        this.AppWindow.Resize(new Windows.Graphics.SizeInt32(640, 600));
        
        this.Title = "设置";
        SettingsFrame.Navigate(typeof(Pages.SettingsPage));
    }
}
