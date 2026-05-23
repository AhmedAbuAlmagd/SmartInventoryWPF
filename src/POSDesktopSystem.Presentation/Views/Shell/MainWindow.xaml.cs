using POSDesktopSystem.Presentation.ViewModels.Shell;
using System.Windows;

namespace POSDesktopSystem.Presentation.Views.Shell;

public partial class MainWindow : Window
{
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
