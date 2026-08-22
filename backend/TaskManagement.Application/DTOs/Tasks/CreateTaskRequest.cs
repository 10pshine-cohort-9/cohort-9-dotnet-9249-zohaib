using System.ComponentModel.DataAnnotations;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs.Tasks;

public class CreateTaskRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string  Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public string? Category { get; set; }

    public DateTime DueDate { get; set; }
    public int? AssignedUserId { get; set; }

}