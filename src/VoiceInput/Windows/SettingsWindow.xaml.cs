using Microsoft.UI.Xaml;

namespace VoiceInput.Windows;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        this.InitializeComponent();
        this.Title = "设置";
        SettingsFrame.Navigate(typeof(Pages.SettingsPage));
    }
}
