using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<IReadOnlyList<User>> GetAllUsersAsync();
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
