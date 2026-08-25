using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Tests.Helpers;

namespace TaskManagement.Tests.Repositories;

public class UserRepositoryTests
{
    [Fact]
    public async Task GetByEmailAsync_ReturnsMatchingUser()
    {
        await using var context = TestDbContextFactory.Create();
        var repository = new UserRepository(context);
        var user = new User
        {
            Id = 1,
            Name = "Jane",
            Email = "jane@example.com",
            Password = "hash",
            Role = UserRole.User
        };

        await repository.AddAsync(user);
        await repository.SaveChangesAsync();

        var result = await repository.GetByEmailAsync("jane@example.com");

        Assert.NotNull(result);
        Assert.Equal("Jane", result!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenUserDoesNotExist()
    {
        await using var context = TestDbContextFactory.Create();
        var repository = new UserRepository(context);

        var result = await repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_PersistsUserToDatabase()
    {
        await using var context = TestDbContextFactory.Create();
        var repository = new UserRepository(context);

        await repository.AddAsync(new User
        {
            Id = 1,
            Name = "Persisted User",
            Email = "persisted@example.com",
            Password = "hash",
            Role = UserRole.User
        });
        await repository.SaveChangesAsync();

        var saved = await repository.GetByIdAsync(1);

        Assert.NotNull(saved);
        Assert.Equal("persisted@example.com", saved!.Email);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsUsersOrderedByName()
    {
        await using var context = TestDbContextFactory.Create();
        var repository = new UserRepository(context);

        await repository.AddAsync(new User { Id = 1, Name = "Charlie", Email = "c@example.com", Password = "h", Role = UserRole.User });
        await repository.AddAsync(new User { Id = 2, Name = "Alice", Email = "a@example.com", Password = "h", Role = UserRole.User });
        await repository.AddAsync(new User { Id = 3, Name = "Bob", Email = "b@example.com", Password = "h", Role = UserRole.Admin });
        await repository.SaveChangesAsync();

        var users = await repository.GetAllUsersAsync();

        Assert.Equal(3, users.Count);
        Assert.Equal("Alice", users[0].Name);
    }
}
