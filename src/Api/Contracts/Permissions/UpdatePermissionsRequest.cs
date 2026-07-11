using Domain.Permissions;
using Domain.Users;

namespace Api.Contracts.Permissions;

public record UpdatePermissionsRequest(IReadOnlyList<PermissionChangeItem> Changes);

public record PermissionChangeItem(Role Rol, Resource Recurso, PermissionAction Accion, bool Permitido);
