using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Application.Services;

public class TaskService: ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<PaginatedResult<TaskDto>> GetTasksAsync(int userId, string role, TaskListQuery query)
    {
        var isAdmin = IsAdmin(role);
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1 , 50);

        var (items, totalCount) = await _taskRepository.GetPagedAsync(isAdmin ? null : userId, isAdmin, query);

        return new PaginatedResult<TaskDto>
        {
            Items = items.Select(MapToTaskDto),
            TotalCount = totalCount,
            Page =  query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<TaskDto> GetTaskByIdAsync(int taskId, int userId, string role)
    {
        var task = await _taskRepository.GetByIdAsync(taskId) ?? throw new NotFoundException("Task not found.");

        if(!IsAdmin(role))
        {
            if(task.AssignedUserId != userId && task.CreatedById != userId)
            {
                throw new ForBiddenException("You don't have accees to this task");
            } 
        }

        return MapToTaskDto(task);
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, int userId, string role)
    {
        int assignedUserId = userId;

        if(IsAdmin(role))
        {
            if(request.AssignedUserId is not int requestedAssigneeId)
            {
                throw new AppException("Assigned user is required");
            }

            var assignee = await _userRepository.GetByIdAsync(requestedAssigneeId) ?? throw new AppException("Assigned user does not exist.");
            assignedUserId = assignee.Id;
        }
        else if(request.AssignedUserId.HasValue && request.AssignedUserId.Value != userId)
        {
            throw new ForBiddenException("Regular users can only create tasks for themselves");
        }

        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = request.Status,
            Priority = request.Priority,
            Category = request.Category?.Trim(),
            DueDate = request.DueDate,
            AssignedUserId = assignedUserId,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();
        return MapToTaskDto(task);
    }

    public async Task<TaskDto> UpdateTaskAsync(int taskId, UpdateTaskRequest request, int userId, string role)
    {
        var task = await _taskRepository.GetByIdAsync(taskId) ?? throw new NotFoundException("Task not found");

        if(!IsAdmin(role))
        {
            if(task.AssignedUserId != userId && task.CreatedById != userId)
            {
                throw new ForBiddenException("You don't have accees to this task");
            } 
        }

        int assignedUserId = task.AssignedUserId;
        if (IsAdmin(role))
        {
            if (request.AssignedUserId is not int requestedAssigneeId)
            {
                throw new AppException("Assigned user is required");
            }

            var assignee = await _userRepository.GetByIdAsync(requestedAssigneeId) ?? throw new AppException("Assigned user does not exist.");
            assignedUserId = assignee.Id;
        }
        else if (request.AssignedUserId.HasValue && request.AssignedUserId.Value != userId)
        {
            throw new ForBiddenException("Regular users can only assign tasks to themselves");
        }
        else
        {
            assignedUserId = userId;
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.Category = request.Category?.Trim();
        task.DueDate = request.DueDate;
        task.AssignedUserId = assignedUserId;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);
        await _taskRepository.SaveChangesAsync();
        return MapToTaskDto(task);

    }

    public async Task DeleteTaskAsync(int taskId, int userId, string role)
    {
        var task = await _taskRepository.GetByIdAsync(taskId) ?? throw new NotFoundException("Task not found");

        if(!IsAdmin(role))
        {
            if(task.AssignedUserId != userId && task.CreatedById != userId)
            {
                throw new ForBiddenException("You don't have accees to this task");
            } 
        }

        await _taskRepository.DeleteAsync(task);
        await _taskRepository.SaveChangesAsync();
    }

    public Task<DashboardStatsDto> GetDashboardStatsAsync(int userId, string role)
    {
        var isAdmin = IsAdmin(role);
        return _taskRepository.GetStatsAsync(isAdmin ? null : userId, isAdmin);

    }

    public async Task<IReadOnlyList<UserDto>> GetAssignableUsersAsync(int userId, string role)
    {
        if(!IsAdmin(role))
        {
            var current = await _userRepository.GetByIdAsync(userId) ?? throw new NotFoundException("User not found");
            return new List<UserDto>
            {
                new()
                { 
                    Id = current.Id, 
                    Name = current.Name,
                    Email = current.Email,
                    Role = current.Role.ToString() 
                }
            };
        }

        var users = await _userRepository.GetAllUsersAsync();
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role.ToString()
        }).ToList();
    }


    private static bool IsAdmin(string role)
    {
        return String.Equals(role, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase);   
    }

    private static TaskDto MapToTaskDto(TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            Category = task.Category,
            DueDate = task.DueDate,
            AssignedUserId = task.AssignedUserId,
            AssignedUserName = task.AssignedUser?.Name ?? string.Empty,
            CreatedById = task.CreatedById,
            CreatedByName = task.CreatedBy?.Name ?? string.Empty,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt    
        };
    }
}

