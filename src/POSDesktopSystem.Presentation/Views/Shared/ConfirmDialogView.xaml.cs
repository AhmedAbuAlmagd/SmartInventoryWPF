using System.Windows;

namespace POSDesktopSystem.Presentation.Views.Shared;

public partial class ConfirmDialogView : Window
{
    public bool Result { get; private set; }

    public ConfirmDialogView(string title, string message)
    {
        InitializeComponent();
        TitleText.Text = title;
        MessageText.Text = message;
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        Result = true;
        DialogResult = true;
        Close();
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        Result = false;
        DialogResult = false;
        Close();
    }
}