using Domain.Permissions;
using Domain.Users;

namespace Application.Permissions;

public record PermissionChange(Role Rol, Resource Recurso, PermissionAction Accion, bool Permitido);
