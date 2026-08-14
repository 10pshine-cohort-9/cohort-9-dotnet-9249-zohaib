using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;


namespace TaskManagement.Application.Services;

public class AuthService: IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository, 
        IRefreshTokenRepository refreshTokenRepository, 
        IPasswordHasher passwordHasher, 
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request) {
        if(!string.Equals(request.Password, request.PasswordConfirm))
        {
            throw new AppException("Passwords do not match");
        }

        var existingEmail = await _userRepository.GetByEmailAsync(request.Email.Trim());
        if(existingEmail is not null)
        {
            throw new AppException("Email is already registered.");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Password = _passwordHasher.HashPassword(request.Password),
            Role = UserRole.User
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return await CreateAuthResponseAsync(user);
    }
    
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim());
        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.Password))
        {
            throw new UnauthorizedException("Invalid Email or Password.");
        }

        return await CreateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _refreshTokenRepository.GetTokenByHashAsync(tokenHash) 
            ?? throw new UnauthorizedException("Invalid or expired refresh token");
        
        await _refreshTokenRepository.RevokeAsync(storedToken);
        await _refreshTokenRepository.SaveChangesAsync();
        return await CreateAuthResponseAsync(storedToken.User);
    } 

    public async Task LogoutAsync(RefreshTokenRequest request)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _refreshTokenRepository.GetTokenByHashAsync(tokenHash);

        if (storedToken is null) return;

        await _refreshTokenRepository.RevokeAsync(storedToken);
        await _refreshTokenRepository.SaveChangesAsync();
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not Found");
        
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),    
        };
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user)
    {
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(refreshTokenValue),
            ExpiresAt = DateTime.UtcNow.AddDays(_tokenService.GetRefreshTokenExpirationDays()),
            CreatedAt = DateTime.UtcNow
        };

        await  _refreshTokenRepository.AddAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = refreshTokenValue
        };
    }

}