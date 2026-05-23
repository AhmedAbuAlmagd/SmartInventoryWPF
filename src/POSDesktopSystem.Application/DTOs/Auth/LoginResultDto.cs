using POSDesktopSystem.Domain.Enums;

namespace POSDesktopSystem.Application.DTOs.Auth;

public class LoginResultDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = default!;
    public UserRole Role { get; set; }
}
