using Moq;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _tokenService.Setup(t => t.GetRefreshTokenExpirationDays()).Returns(7);
        _sut = new AuthService(
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _passwordHasher.Object,
            _tokenService.Object);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsWhenPasswordsDoNotMatch()
    {
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password1!",
            PasswordConfirm = "Different1!"
        };

        await Assert.ThrowsAsync<AppException>(() => _sut.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_ThrowsWhenEmailAlreadyExists()
    {
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User { Email = "test@example.com" });

        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password1!",
            PasswordConfirm = "Password1!"
        };

        await Assert.ThrowsAsync<AppException>(() => _sut.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_CreatesUserAndReturnsTokens()
    {
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("hashed-password");
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");
        _tokenService.Setup(t => t.HashRefreshToken("refresh-token")).Returns("refresh-hash");

        var request = new RegisterRequest
        {
            Name = "New User",
            Email = "new@example.com",
            Password = "Password1!",
            PasswordConfirm = "Password1!"
        };

        var result = await _sut.RegisterAsync(request);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ThrowsWhenCredentialsInvalid()
    {
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = "test@example.com", Password = "wrong" }));
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokensWhenCredentialsValid()
    {
        var user = new User { Id = 1, Name = "Test", Email = "test@example.com", Role = UserRole.User };

        _userRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.VerifyPassword("Password1!", user.Password)).Returns(true);
        _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("access-token");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");
        _tokenService.Setup(t => t.HashRefreshToken("refresh-token")).Returns("refresh-hash");

        var result = await _sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "Password1!" });

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_ReturnsNewTokensWhenValid()
    {
        var user = new User { Id = 1, Name = "Test", Email = "test@example.com", Role = UserRole.User };
        var storedToken = new RefreshToken { Id = 1, UserId = 1, User = user, TokenHash = "hash", ExpiresAt = DateTime.UtcNow.AddDays(1) };

        _tokenService.Setup(t => t.HashRefreshToken("old-refresh")).Returns("hash");
        _refreshTokenRepository.Setup(r => r.GetTokenByHashAsync("hash")).ReturnsAsync(storedToken);
        _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("new-access");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh");
        _tokenService.Setup(t => t.HashRefreshToken("new-refresh")).Returns("new-hash");

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "old-refresh" });

        Assert.Equal("new-access", result.AccessToken);
        Assert.Equal("new-refresh", result.RefreshToken);
        _refreshTokenRepository.Verify(r => r.RevokeAsync(storedToken), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ThrowsWhenUserNotFound()
    {
        _userRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetCurrentUserAsync(99));
    }

    [Fact]
    public async Task GetCurrentUserAsync_ReturnsUserDto()
    {
        var user = new User { Id = 1, Name = "Jane", Email = "jane@example.com", Role = UserRole.Admin };
        _userRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _sut.GetCurrentUserAsync(1);

        Assert.Equal(1, result.Id);
        Assert.Equal("Admin", result.Role);
    }
}