using Application.Audit;
using Domain.Audit;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _db;

    public AuditLogRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync() => await _db.AuditLogs.ToListAsync();

    public async Task DeleteOlderThanAsync(DateTime cutoffUtc)
    {
        await _db.AuditLogs.Where(a => a.CreatedAt < cutoffUtc).ExecuteDeleteAsync();
    }
}
