using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories;

public class TaskRepository: ITaskRepository
{
    private readonly AppDbContext  _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<TaskItem?> GetByIdAsync(int id)
    {
        return _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetPagedAsync(int? userId, bool isAdmin, TaskListQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        
        var q = _context.Tasks
                    .Include(t => t.AssignedUser)
                    .Include(t => t.CreatedBy)
                    .AsQueryable();

        if(!isAdmin && userId.HasValue)
        {
            q = q.Where(t => t.AssignedUserId == userId.Value || t.CreatedById == userId.Value);
        }

        if(!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(t => t.Title.ToLower().Contains(search) || 
                        (t.Description != null && t.Description.ToLower().Contains(search)) ||
                        (t.Category != null && t.Category.ToLower().Contains(search)));
        }

        if(query.Status.HasValue)
        {
            q = q.Where(t => t.Status == query.Status.Value);
        }

        if(query.Priority.HasValue)
        {
            q = q.Where(t => t.Priority == query.Priority.Value);
        }

        if(!string.IsNullOrWhiteSpace(query.Category))
        {
            q = q.Where(t => t.Category != null && t.Category.ToLower() == query.Category.Trim().ToLower());
        }

        var totalCount = await q.CountAsync();

        var items = await q.OrderByDescending(t => t.CreatedAt).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

        return (items, totalCount);
    } 

    public async Task<DashboardStatsDto> GetStatsAsync(int? userId, bool isAdmin)
    {
        var q =  _context.Tasks.AsQueryable();

        if(!isAdmin && userId.HasValue)
        {
            q = q.Where(t => t.AssignedUserId == userId.Value || t.CreatedById == userId.Value);
        }

        var stats = await q
                            .GroupBy(_ => 1)
                            .Select(g => new DashboardStatsDto
                            {
                                PendingCount = g.Count(t => t.Status == TaskItemStatus.Pending),
                                InProgressCount = g.Count(t => t.Status == TaskItemStatus.InProgress),
                                CompletedCount = g.Count(t => t.Status == TaskItemStatus.Completed)
                            })
                            .FirstOrDefaultAsync();
        
        return stats ?? new DashboardStatsDto();
    }

    public async Task AddAsync(TaskItem task)
    {
        await _context.Tasks.AddAsync(task);
    }

    public Task UpdateAsync(TaskItem task)
    {
        _context.Tasks.Update(task);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TaskItem task)
    {
        _context.Tasks.Remove(task);
        return Task.CompletedTask;
    }


    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    } 


}

