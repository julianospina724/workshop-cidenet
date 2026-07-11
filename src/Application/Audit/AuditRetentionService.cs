using Application.Common;

namespace Application.Audit;

/// <summary>
/// Política de retención de US-008-AUD: los registros de auditoría se
/// conservan solo 6 meses. Sin UI ni endpoint HTTP en este MVP — se invoca
/// como una rutina de mantenimiento (job/función), no desde una petición de usuario.
/// </summary>
public class AuditRetentionService
{
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(180);

    private readonly IAuditLogRepository _repository;
    private readonly IClock _clock;

    public AuditRetentionService(IAuditLogRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public Task PurgeOldEntriesAsync()
    {
        var cutoff = _clock.UtcNow - RetentionPeriod;
        return _repository.DeleteOlderThanAsync(cutoff);
    }
}
