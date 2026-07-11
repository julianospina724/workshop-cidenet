using Application.Common;
using Domain.Audit;
using Domain.Permissions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserAccessor currentUserAccessor) : base(options)
    {
        _currentUserAccessor = currentUserAccessor;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<PermissionMatrixEntry> PermissionMatrixEntries => Set<PermissionMatrixEntry>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<PermissionMatrixEntry>().HasData(DefaultPermissionMatrix.Entries);
        modelBuilder.Entity<User>().HasData(DefaultAdminSeed.Entry);
    }

    /// <summary>
    /// Genera el registro de auditoría (US-008-AUD) de forma atómica con el
    /// cambio que audita: ambos se insertan en el mismo SaveChanges, dentro de
    /// la misma transacción implícita — si uno falla, el otro tampoco se persiste.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = BuildAuditEntries();
        if (auditEntries.Count > 0)
        {
            await AuditLogs.AddRangeAsync(auditEntries, cancellationToken);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private List<AuditLog> BuildAuditEntries()
    {
        var performedBy = _currentUserAccessor.CurrentUserId ?? Guid.Empty;
        var entries = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries<User>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                entries.Add(new AuditLog
                {
                    PerformedByUserId = performedBy,
                    EntityName = nameof(User),
                    Action = "create",
                    Details = $"Nombre={entry.Entity.Nombre}, Email={entry.Entity.Email}, Rol={entry.Entity.Rol}",
                });
                continue;
            }

            var estadoProperty = entry.Property(u => u.Estado);
            var esEliminacionLogica = estadoProperty.IsModified && entry.Entity.Estado == UserStatus.Eliminado;

            var cambios = string.Join(
                "; ",
                entry.Properties.Where(p => p.IsModified).Select(p => $"{p.Metadata.Name}: {p.OriginalValue} -> {p.CurrentValue}"));

            entries.Add(new AuditLog
            {
                PerformedByUserId = performedBy,
                EntityName = nameof(User),
                Action = esEliminacionLogica ? "delete" : "update",
                Details = cambios,
            });
        }

        foreach (var entry in ChangeTracker.Entries<PermissionMatrixEntry>())
        {
            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            entries.Add(new AuditLog
            {
                PerformedByUserId = performedBy,
                EntityName = nameof(PermissionMatrixEntry),
                Action = "update",
                Details = $"{entry.Entity.Rol}/{entry.Entity.Recurso}/{entry.Entity.Accion} -> Permitido={entry.Entity.Permitido}",
            });
        }

        return entries;
    }
}
