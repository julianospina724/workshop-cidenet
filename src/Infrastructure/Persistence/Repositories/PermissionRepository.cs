using Application.Permissions;
using Domain.Permissions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _db;

    public PermissionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PermissionMatrixEntry>> GetAllAsync() =>
        await _db.PermissionMatrixEntries.ToListAsync();

    public Task<PermissionMatrixEntry?> FindEntryAsync(Role rol, Resource recurso, PermissionAction accion) =>
        _db.PermissionMatrixEntries.SingleOrDefaultAsync(e => e.Rol == rol && e.Recurso == recurso && e.Accion == accion);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
