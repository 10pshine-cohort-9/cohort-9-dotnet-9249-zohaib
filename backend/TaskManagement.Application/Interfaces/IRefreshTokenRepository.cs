using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetTokenByHashAsync(string TokenHash);
    Task AddAsync(RefreshToken refreshToken);
    Task RevokeAsync(RefreshToken refreshToken);
    Task SaveChangesAsync();
}