using Domain.Audit;

namespace Application.Audit;

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditLog>> GetAllAsync();
    Task DeleteOlderThanAsync(DateTime cutoffUtc);
}
