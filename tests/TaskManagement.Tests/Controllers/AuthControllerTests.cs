using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.API.Controllers;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Interfaces;
using TaskManagement.Tests.Helpers;

namespace TaskManagement.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authService = new();
    private readonly Mock<ILogger<AuthController>> _logger = new();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _sut = new AuthController(_authService.Object, _logger.Object);
    }

    [Fact]
    public async Task Register_ReturnsOkWithAuthResponse()
    {
        var request = new RegisterRequest
        {
            Name = "User",
            Email = "user@example.com",
            Password = "Password1!",
            PasswordConfirm = "Password1!"
        };

        var response = new AuthResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        };

        _authService.Setup(s => s.RegisterAsync(request)).ReturnsAsync(response);

        var result = await _sut.Register(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("access-token", payload.AccessToken);
    }

    [Fact]
    public async Task Login_ReturnsOkWithAuthResponse()
    {
        var request = new LoginRequest { Email = "user@example.com", Password = "Password1!" };
        var response = new AuthResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        };

        _authService.Setup(s => s.LoginAsync(request)).ReturnsAsync(response);

        var result = await _sut.Login(request);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Refresh_ReturnsOkWithNewTokens()
    {
        var request = new RefreshTokenRequest { RefreshToken = "old-refresh" };
        var response = new AuthResponse
        {
            AccessToken = "new-access",
            RefreshToken = "new-refresh"
        };

        _authService.Setup(s => s.RefreshTokenAsync(request)).ReturnsAsync(response);

        var result = await _sut.Refresh(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("new-access", ((AuthResponse)ok.Value!).AccessToken);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsOkWithUserDto()
    {
        const int userId = 1;
        ControllerTestHelper.SetUser(_sut, userId, "User");

        var user = new UserDto { Id = userId, Name = "User", Email = "user@example.com", Role = "User" };
        _authService.Setup(s => s.GetCurrentUserAsync(userId)).ReturnsAsync(user);

        var result = await _sut.GetCurrentUser();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(userId, ((UserDto)ok.Value!).Id);
    }
}