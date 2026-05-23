using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Presentation.ViewModels.Shell;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace POSDesktopSystem.Presentation.Infrastructure;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private ShellViewModel? _shell;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public string CurrentView { get; private set; } = string.Empty;

    public void NavigateTo(string viewKey)
    {
        if (viewKey == Constants.NavigationKeys.Shell)
        {
            var shellVm = _serviceProvider.GetRequiredService<ShellViewModel>();
            var mainWindow = new Views.Shell.MainWindow(shellVm);
            System.Windows.Application.Current.MainWindow = mainWindow;
            mainWindow.Show();

            // Set default view for Shell
            _shell = shellVm;
            NavigateTo(Constants.NavigationKeys.Pos);

            // Close all other windows (like LoginWindow)
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window != mainWindow) window.Close();
            }
            return;
        }

        _shell ??= _serviceProvider.GetRequiredService<ShellViewModel>();
        CurrentView = viewKey;
        
        object? vm = viewKey switch
        {
            Constants.NavigationKeys.Pos => _serviceProvider.GetRequiredService<ViewModels.POS.PosViewModel>(),
            Constants.NavigationKeys.Products => _serviceProvider.GetRequiredService<ViewModels.Products.ProductsViewModel>(),
            Constants.NavigationKeys.Invoices => _serviceProvider.GetRequiredService<ViewModels.Invoices.InvoicesViewModel>(),
            _ => null
        };
        
        if (vm != null)
        {
            _shell.CurrentViewModel = (ViewModels.Base.BaseViewModel)vm;
            _shell.CurrentViewTitle = viewKey;
        }
    }

    public void GoBack() { }
}
