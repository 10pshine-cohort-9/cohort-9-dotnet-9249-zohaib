using Moq;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _sut = new TaskService(_taskRepository.Object, _userRepository.Object);
    }

    [Fact]
    public async Task GetTasksAsync_ReturnsPagedResults()
    {
        const int userId = 2;
        var tasks = new List<TaskItem>
        {
            new()
            {
                Id = 1,
                Title = "Task 1",
                AssignedUserId = userId,
                CreatedById = userId,
                AssignedUser = new User { Name = "User" },
                CreatedBy = new User { Name = "User" }
            }
        };

        _taskRepository.Setup(r => r.GetPagedAsync(userId, false, It.IsAny<TaskListQuery>()))
            .ReturnsAsync((tasks, 1));

        var result = await _sut.GetTasksAsync(userId, UserRole.User.ToString(), new TaskListQuery { Page = 1, PageSize = 10 });

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ThrowsWhenUserHasNoAccess()
    {
        const int userId = 2;
        var taskId = 1;
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Private",
            AssignedUserId = 3,
            CreatedById = 4
        };

        _taskRepository.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);

        await Assert.ThrowsAsync<ForBiddenException>(() =>
            _sut.GetTaskByIdAsync(taskId, userId, UserRole.User.ToString()));
    }

    [Fact]
    public async Task GetTaskByIdAsync_AllowsAdminToAccessAnyTask()
    {
        var taskId = 1;
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Any Task",
            AssignedUserId = 2,
            CreatedById = 1,
            AssignedUser = new User { Name = "Assignee" },
            CreatedBy = new User { Name = "Creator" }
        };

        _taskRepository.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);

        var result = await _sut.GetTaskByIdAsync(taskId, 99, UserRole.Admin.ToString());

        Assert.Equal("Any Task", result.Title);
    }

    [Fact]
    public async Task CreateTaskAsync_ForRegularUser_AssignsTaskToSelf()
    {
        const int userId = 2;
        _taskRepository.Setup(r => r.AddAsync(It.IsAny<TaskItem>()))
            .Callback<TaskItem>(task => task.Id = 1);
        _taskRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new TaskItem
            {
                Id = 1,
                Title = "My Task",
                AssignedUserId = userId,
                CreatedById = userId,
                AssignedUser = new User { Name = "User" },
                CreatedBy = new User { Name = "User" }
            });

        var result = await _sut.CreateTaskAsync(new CreateTaskRequest { Title = "My Task", AssignedUserId = userId }, userId, UserRole.User.ToString());

        Assert.Equal(userId, result.AssignedUserId);
    }

    [Fact]
    public async Task CreateTaskAsync_ForRegularUser_ThrowsWhenAssigningToOtherUser()
    {
        const int userId = 2;
        var request = new CreateTaskRequest { Title = "Task", AssignedUserId = 3 };

        await Assert.ThrowsAsync<ForBiddenException>(() =>
            _sut.CreateTaskAsync(request, userId, UserRole.User.ToString()));
    }

    [Fact]
    public async Task CreateTaskAsync_ForAdmin_AssignsToRequestedUser()
    {
        const int adminId = 1;
        const int assigneeId = 2;

        _userRepository.Setup(r => r.GetByIdAsync(assigneeId))
            .ReturnsAsync(new User { Id = assigneeId, Name = "Assignee" });
        _taskRepository.Setup(r => r.AddAsync(It.IsAny<TaskItem>()))
            .Callback<TaskItem>(task => task.Id = 1);
        _taskRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new TaskItem
            {
                Id = 1,
                Title = "Admin Task",
                AssignedUserId = assigneeId,
                CreatedById = adminId,
                AssignedUser = new User { Name = "Assignee" },
                CreatedBy = new User { Name = "Admin" }
            });

        var result = await _sut.CreateTaskAsync(
            new CreateTaskRequest { Title = "Admin Task", AssignedUserId = assigneeId },
            adminId,
            UserRole.Admin.ToString());

        Assert.Equal(assigneeId, result.AssignedUserId);
    }

    [Fact]
    public async Task DeleteTaskAsync_DeletesTaskSuccessfully()
    {
        const int userId = 2;
        var taskId = 1;
        var task = new TaskItem { Id = taskId, Title = "To Delete", AssignedUserId = userId, CreatedById = userId };

        _taskRepository.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);

        await _sut.DeleteTaskAsync(taskId, userId, UserRole.User.ToString());

        _taskRepository.Verify(r => r.DeleteAsync(task), Times.Once);
    }

    [Fact]
    public async Task GetAssignableUsersAsync_ForUser_ReturnsOnlyCurrentUser()
    {
        const int userId = 2;
        _userRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, Name = "Self", Email = "self@example.com", Role = UserRole.User });

        var result = await _sut.GetAssignableUsersAsync(userId, UserRole.User.ToString());

        Assert.Single(result);
        Assert.Equal(userId, result[0].Id);
    }
}
