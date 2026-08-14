using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);

    int GetRefreshTokenExpirationDays();
}

