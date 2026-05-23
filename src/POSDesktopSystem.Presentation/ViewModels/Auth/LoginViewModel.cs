using POSDesktopSystem.Application.DTOs.Auth;
using POSDesktopSystem.Application.Exceptions;
using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Presentation.Constants;
using POSDesktopSystem.Presentation.ViewModels.Base;
using POSDesktopSystem.Presentation.Infrastructure;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace POSDesktopSystem.Presentation.ViewModels.Auth;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly ISessionService _sessionService;
    private readonly INavigationService _navigationService;

    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set 
        {
            if (SetProperty(ref _username, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set 
        {
            if (SetProperty(ref _password, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    private bool _isPasswordVisible;
    public bool IsPasswordVisible
    {
        get => _isPasswordVisible;
        set => SetProperty(ref _isPasswordVisible, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand TogglePasswordVisibilityCommand { get; }

    public LoginViewModel(IAuthService authService, ISessionService sessionService, INavigationService navigationService)
    {
        _authService = authService;
        _sessionService = sessionService;
        _navigationService = navigationService;

        // Make command always executable so the button is always clickable
        LoginCommand = new AsyncRelayCommand(LoginAsync);
        TogglePasswordVisibilityCommand = new RelayCommand(_ => {
            IsPasswordVisible = !IsPasswordVisible;
            OnPropertyChanged(nameof(Password)); // Force sync
        });
    }

    private async Task LoginAsync(object? parameter)
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        { 
            ErrorMessage = "Please enter both username and password.";
            return;
        }

        ErrorMessage = "Attempting to login...";
        IsBusy = true;

        try
        {
            var request = new LoginRequestDto { Username = Username, Password = Password };
            var result = await _authService.LoginAsync(request);
            
            _sessionService.SetUser(result);
            _navigationService.NavigateTo(NavigationKeys.Shell);

            // Note: LoginWindow is closed dynamically when Shell opens (handled by View/App)
        }
        catch (UnauthorizedException ex)
        {
            ErrorMessage = "Login Failed: " + ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = "System Error: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
