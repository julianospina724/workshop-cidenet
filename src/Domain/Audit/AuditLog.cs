using Domain.Common;

namespace Domain.Audit;

public class AuditLog : Entity
{
    public required Guid PerformedByUserId { get; set; }
    public required string EntityName { get; set; }
    public required string Action { get; set; }
    public string? Details { get; set; }
}
