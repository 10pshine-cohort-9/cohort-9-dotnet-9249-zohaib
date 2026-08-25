using System.ComponentModel.DataAnnotations;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs.Tasks;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string? Category { get; set; }
    public DateTime DueDate { get; set; }
    public int AssignedUserId { get; set; }
    public string AssignedUserName { get; set; } = string.Empty;

    public int CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

}