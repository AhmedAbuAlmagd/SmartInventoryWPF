using POSDesktopSystem.Application.DTOs.Auth;
using POSDesktopSystem.Application.Exceptions;
using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Domain.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace POSDesktopSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<LoginResultDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(dto.Username);
        if (user == null)
        {
            _logger.LogWarning("Failed login attempt for unknown user: {Username}", dto.Username);
            throw new UnauthorizedException("Invalid credentials");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for disabled account: {Username}", dto.Username);
            throw new UnauthorizedException("Account is disabled");
        }

        bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isValid)
        {
            _logger.LogWarning("Failed login attempt (wrong password) for user: {Username}", dto.Username);
            throw new UnauthorizedException("Invalid credentials");
        }

        _logger.LogInformation("User logged in successfully: {Username} (Role: {Role})", user.Username, user.Role);

        return new LoginResultDto
        {
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }
}
