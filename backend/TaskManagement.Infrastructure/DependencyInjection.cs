using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>

            options.UseSqlServer(connectionString));

        services.AddScoped<IUserRepository, Repositories.UserRepository>();
        services.AddScoped<IRefreshTokenRepository, Repositories.RefreshTokenRepository>();
        services.AddScoped<ITokenService, Security.TokenService>();
        services.AddScoped<IPasswordHasher, Security.PasswordHasher>();
        return services;
    }

    public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
          

        if (await context.Users.AnyAsync()) return;

        var admin  = new User
        {
            Name = "Admin",
            Email = "admin@taskmanagement.com",
            Password = passwordHasher.HashPassword("Admin@123"),
            Role = UserRole.Admin,
        };

        var regularUser = new User
        {
            Name = "John",
            Email = "johndoe@gmail.com",
            Password = passwordHasher.HashPassword("john@123"),
            Role = UserRole.User,
        };

        context.Users.AddRange(admin, regularUser);
        await context.SaveChangesAsync();

        logger.LogInformation("Database seeded with default admin and user");

    }
}


