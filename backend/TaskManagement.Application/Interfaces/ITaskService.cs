using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<PaginatedResult<TaskDto>> GetTasksAsync(int userId, string role, TaskListQuery query);
    Task<TaskDto> GetTaskByIdAsync(int taskId, int userId, string role);

    Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, int userId, string role);
    Task<TaskDto> UpdateTaskAsync(int taskId, UpdateTaskRequest request, int userId, string role);
    Task DeleteTaskAsync(int taskId, int userId, string role);
    Task<DashboardStatsDto> GetDashboardStatsAsync(int userId, string role);
    Task<IReadOnlyList<UserDto>> GetAssignableUsersAsync(int userId, string role);
}