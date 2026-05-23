using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Presentation.Constants;
using POSDesktopSystem.Presentation.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;
using System.Windows;

namespace POSDesktopSystem.Presentation.ViewModels.Shell;

public class ShellViewModel : BaseViewModel
{
    private readonly ISessionService _sessionService;
    private readonly INavigationService _navigationService;

    private BaseViewModel? _currentViewModel;
    public BaseViewModel? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    private string _currentViewTitle = string.Empty;
    public string CurrentViewTitle
    {
        get => _currentViewTitle;
        set => SetProperty(ref _currentViewTitle, value);
    }

    public bool IsManagerRole => _sessionService.IsManager;
    public string CurrentUsername => _sessionService.CurrentUser?.Username ?? "Unknown";

    public ICommand NavigateToPosCommand { get; }
    public ICommand NavigateToProductsCommand { get; }
    public ICommand NavigateToInvoicesCommand { get; }
    public ICommand LogoutCommand { get; }

    public ShellViewModel(ISessionService sessionService, INavigationService navigationService)
    {
        _sessionService = sessionService;
        _navigationService = navigationService;

        NavigateToPosCommand = new RelayCommand(_ => _navigationService.NavigateTo(NavigationKeys.Pos));
        NavigateToProductsCommand = new RelayCommand(
            _ => _navigationService.NavigateTo(NavigationKeys.Products),
            _ => IsManagerRole
        );
        NavigateToInvoicesCommand = new RelayCommand(_ => _navigationService.NavigateTo(NavigationKeys.Invoices));
        
        LogoutCommand = new RelayCommand(_ => Logout());
    }

    private void Logout()
    {
        _sessionService.Clear();
        var app = (App)System.Windows.Application.Current;
        var loginViewModel = app.Services.GetRequiredService<ViewModels.Auth.LoginViewModel>();
        var loginWindow = new Views.Auth.LoginWindow(loginViewModel);
        loginWindow.Show();
        System.Windows.Application.Current.MainWindow.Close();
    }
}
