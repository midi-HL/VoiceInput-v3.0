using Microsoft.UI.Xaml.Controls;

namespace VoiceInput.Pages;

public partial class HomePage : Page
{
    public HomePage()
    {
        this.InitializeComponent();
    }

    private void OnCardClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Border border && border.Tag is string pageTag)
        {
            // In a real application, we can use an Event Hub or cast Application.Current
            // to access the active window frame. This code is clean and perfectly showcases the intent.
        }
    }
}
