using POSDesktopSystem.Application.DTOs.Auth;
using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Domain.Enums;

namespace POSDesktopSystem.Application.Services;

public class SessionService : ISessionService
{
    private LoginResultDto? _currentUser;

    public LoginResultDto? CurrentUser => _currentUser;

    public bool IsLoggedIn => _currentUser != null;

    public bool IsManager => _currentUser?.Role == UserRole.Manager;

    public void SetUser(LoginResultDto user)
    {
        _currentUser = user;
    }

    public void Clear()
    {
        _currentUser = null;
    }
}
