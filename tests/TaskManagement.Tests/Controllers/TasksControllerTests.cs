using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.API.Controllers;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Enums;
using TaskManagement.Tests.Helpers;

namespace TaskManagement.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _taskService = new();
    private readonly Mock<ILogger<TasksController>> _logger = new();
    private readonly TasksController _sut;
    private const int UserId = 2;

    public TasksControllerTests()
    {
        _sut = new TasksController(_taskService.Object, _logger.Object);
        ControllerTestHelper.SetUser(_sut, UserId, UserRole.User.ToString());
    }

    [Fact]
    public async Task GetTasks_ReturnsOkWithPagedResult()
    {
        var paged = new PaginatedResult<TaskDto>
        {
            Items = new[] { new TaskDto { Title = "Task 1", Status = "Pending", Priority = "Medium" } },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _taskService.Setup(s => s.GetTasksAsync(UserId, UserRole.User.ToString(), It.IsAny<TaskListQuery>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetTasks(new TaskListQuery());

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(((PaginatedResult<TaskDto>)ok.Value!).Items);
    }

    [Fact]
    public async Task CreateTask_ReturnsCreatedAtAction()
    {
        var request = new CreateTaskRequest { Title = "New Task" };
        var created = new TaskDto { Id = 1, Title = "New Task", Status = "Pending", Priority = "Medium" };

        _taskService.Setup(s => s.CreateTaskAsync(request, UserId, UserRole.User.ToString()))
            .ReturnsAsync(created);

        var result = await _sut.CreateTask(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(created.Id, createdResult.RouteValues!["id"]);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNoContent()
    {
        var taskId = 1;
        _taskService.Setup(s => s.DeleteTaskAsync(taskId, UserId, UserRole.User.ToString()))
            .Returns(Task.CompletedTask);

        var result = await _sut.DeleteTask(taskId);

        Assert.IsType<NoContentResult>(result);
    }
}
