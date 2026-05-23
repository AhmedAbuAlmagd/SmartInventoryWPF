using POSDesktopSystem.Application.DTOs.Auth;

namespace POSDesktopSystem.Application.Interfaces;

public interface ISessionService
{
    LoginResultDto? CurrentUser { get; }
    bool IsLoggedIn { get; }
    bool IsManager { get; }
    void SetUser(LoginResultDto user);
    void Clear();
}
