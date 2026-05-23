using Moq;
using Microsoft.Extensions.Logging;
using POSDesktopSystem.Application.DTOs.Auth;
using POSDesktopSystem.Application.Exceptions;
using POSDesktopSystem.Application.Services;
using POSDesktopSystem.Domain.Entities;
using POSDesktopSystem.Domain.Enums;
using POSDesktopSystem.Domain.Interfaces;
using Xunit;

namespace POSDesktopSystem.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _service = new AuthService(_uowMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResult()
    {
        // Arrange
        var password = "Password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = passwordHash,
            Role = UserRole.Cashier,
            IsActive = true
        };

        _uowMock.Setup(x => x.Users.GetByUsernameAsync(user.Username))
            .ReturnsAsync(user);

        var request = new LoginRequestDto { Username = user.Username, Password = password };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.Username, result.Username);
        Assert.Equal(user.Role, result.Role);
    }

    [Fact]
    public async Task LoginAsync_InvalidUsername_ThrowsUnauthorizedException()
    {
        // Arrange
        _uowMock.Setup(x => x.Users.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var request = new LoginRequestDto { Username = "nonexistent", Password = "password" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(request));
        Assert.Equal("Invalid credentials", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User { Username = "inactive", IsActive = false };
        _uowMock.Setup(x => x.Users.GetByUsernameAsync(user.Username))
            .ReturnsAsync(user);

        var request = new LoginRequestDto { Username = user.Username, Password = "password" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(request));
        Assert.Equal("Account is disabled", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
            IsActive = true
        };

        _uowMock.Setup(x => x.Users.GetByUsernameAsync(user.Username))
            .ReturnsAsync(user);

        var request = new LoginRequestDto { Username = user.Username, Password = "WrongPassword" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(request));
        Assert.Equal("Invalid credentials", ex.Message);
    }
}
