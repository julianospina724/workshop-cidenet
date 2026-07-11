using Domain.Permissions;
using Domain.Users;

namespace Application.Permissions;

public interface IPermissionRepository
{
    Task<IReadOnlyList<PermissionMatrixEntry>> GetAllAsync();
    Task<PermissionMatrixEntry?> FindEntryAsync(Role rol, Resource recurso, PermissionAction accion);
    Task SaveChangesAsync();
}
