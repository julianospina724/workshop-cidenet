using Domain.Permissions;
using Domain.Users;

namespace Api.Contracts.Permissions;

public record PermissionEntryResponse(Role Rol, Resource Recurso, PermissionAction Accion, bool Permitido)
{
    public static PermissionEntryResponse FromEntity(PermissionMatrixEntry entry) =>
        new(entry.Rol, entry.Recurso, entry.Accion, entry.Permitido);
}
