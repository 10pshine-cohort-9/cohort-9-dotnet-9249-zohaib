using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManagement.Domain.Enums;


namespace TaskManagement.Domain.Entities;

public class User {
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Email { get; set;} = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set;} = UserRole.User;

    public DateTime createdAt {get; set; } = DateTime.UtcNow;

    public ICollection<RefreshToken> RefreshTokens {get; set; } = new List<RefreshToken>();
    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();

}

