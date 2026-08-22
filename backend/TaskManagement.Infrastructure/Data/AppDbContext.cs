using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;


namespace TaskManagement.Infrastructure.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity => 
        {
            entity.Property(u => u.Id).UseIdentityColumn();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<RefreshToken>(entity => 
        {
            entity.Property(r => r.Id).UseIdentityColumn();
            entity.HasIndex(r => r.TokenHash);

            entity.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(t => t.Status).HasConversion<string>();
            entity.Property(t => t.Priority).HasConversion<string>();

            entity.HasOne(t => t.AssignedUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.CreatedBy)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}




