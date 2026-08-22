using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;

namespace TaskManagement.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TasksController: ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }


    [HttpGet]
    public async Task<ActionResult<TaskDto>> GetTasks([FromQuery] TaskListQuery query)
    {
        int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        var result = await _taskService.GetTasksAsync(userId, role, query);
        return Ok(result);
    }

    [HttpGet("dashboard-stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";

        var stats = await _taskService.GetDashboardStatsAsync(userId, role);
        return Ok(stats);
    }

    [HttpGet("assignable-users")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAssignableUsers()
    {
        int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        var users = await _taskService.GetAssignableUsersAsync(userId, role);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        var task = await _taskService.GetTaskByIdAsync(id, userId, role);
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskRequest request)
    {
        int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        _logger.LogInformation("Creating Task {Title} by user {userId}", request.Title, userId);
        var task = await _taskService.CreateTaskAsync(request, userId, role);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
    {
        int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        _logger.LogInformation("Updaing Task {TaskId} by user {userId}", id, userId);
        var task = await _taskService.UpdateTaskAsync(id, request, userId, role);
        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        _logger.LogInformation("Deleting task {TaskId} by user {UserId}", id, userId);
        await _taskService.DeleteTaskAsync(id, userId, role);
        return NoContent();
    }    
}
