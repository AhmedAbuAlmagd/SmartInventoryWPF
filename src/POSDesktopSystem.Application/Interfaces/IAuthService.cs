using POSDesktopSystem.Application.DTOs.Auth;
using System.Threading.Tasks;

namespace POSDesktopSystem.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginRequestDto dto);
}
