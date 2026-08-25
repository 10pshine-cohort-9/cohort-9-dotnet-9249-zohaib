
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManagement.Domain.Enums;
namespace TaskManagement.Domain.Entities;

public class TaskItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public string? Category { get; set; }

    public DateTime DueDate { get; set; }

    public int AssignedUserId { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public User AssignedUser { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;

}