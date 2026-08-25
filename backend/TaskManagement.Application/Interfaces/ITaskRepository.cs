using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(int id);
    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetPagedAsync(int? userId, bool isAdmin, TaskListQuery query);
    Task<DashboardStatsDto> GetStatsAsync(int? userId, bool isAdmin);
    Task AddAsync(TaskItem task); 
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
    Task SaveChangesAsync();
}