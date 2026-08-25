using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Tests.Helpers;

namespace TaskManagement.Tests.Repositories;

public class TaskRepositoryTests
{
    private static async Task<(User Owner, User Other, TaskRepository Repository)> SeedTasksAsync()
    {
        var context = TestDbContextFactory.Create();
        var repository = new TaskRepository(context);

        var owner = new User { Id = 1, Name = "Owner", Email = "owner@example.com", Password = "hash", Role = UserRole.User };
        var other = new User { Id = 2, Name = "Other", Email = "other@example.com", Password = "hash", Role = UserRole.User };

        context.Users.AddRange(owner, other);
        context.Tasks.AddRange(
            new TaskItem
            {
                Title = "Owner Pending Task",
                Status = TaskItemStatus.Pending,
                Priority = TaskPriority.Medium,
                Category = "Development",
                AssignedUserId = owner.Id,
                CreatedById = owner.Id,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new TaskItem
            {
                Title = "Owner Completed Task",
                Description = "Important work",
                Status = TaskItemStatus.Completed,
                Priority = TaskPriority.High,
                Category = "Testing",
                AssignedUserId = owner.Id,
                CreatedById = owner.Id,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new TaskItem
            {
                Title = "Other User Task",
                Status = TaskItemStatus.InProgress,
                Priority = TaskPriority.Low,
                Category = "Development",
                AssignedUserId = other.Id,
                CreatedById = other.Id,
                CreatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync();
        return (owner, other, repository);
    }

    [Fact]
    public async Task GetPagedAsync_ForRegularUser_ReturnsOnlyOwnTasks()
    {
        var (owner, _, repository) = await SeedTasksAsync();

        var (items, totalCount) = await repository.GetPagedAsync(owner.Id, false, new TaskListQuery { Page = 1, PageSize = 10 });

        Assert.Equal(2, totalCount);
        Assert.All(items, t => Assert.Equal(owner.Id, t.AssignedUserId));
    }

    [Fact]
    public async Task GetPagedAsync_ForAdmin_ReturnsAllTasks()
    {
        var (_, _, repository) = await SeedTasksAsync();

        var (items, totalCount) = await repository.GetPagedAsync(null, true, new TaskListQuery { Page = 1, PageSize = 10 });

        Assert.Equal(3, totalCount);
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsCorrectCountsForUser()
    {
        var (owner, _, repository) = await SeedTasksAsync();

        var stats = await repository.GetStatsAsync(owner.Id, false);

        Assert.Equal(1, stats.PendingCount);
        Assert.Equal(1, stats.CompletedCount);
    }
}
