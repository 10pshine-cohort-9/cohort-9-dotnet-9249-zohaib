using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task LogoutAsync(RefreshTokenRequest request);
    Task<UserDto> GetCurrentUserAsync(int userId);

}
