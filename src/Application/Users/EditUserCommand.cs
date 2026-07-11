using Domain.Users;

namespace Application.Users;

public record EditUserCommand(Guid TargetUserId, string? Nombre, string? Email, Role? Rol, UserStatus? Estado);
