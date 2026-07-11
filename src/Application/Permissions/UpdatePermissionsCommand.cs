using Domain.Users;

namespace Application.Permissions;

public record UpdatePermissionsCommand(Role CallerRole, IReadOnlyList<PermissionChange> Changes);
