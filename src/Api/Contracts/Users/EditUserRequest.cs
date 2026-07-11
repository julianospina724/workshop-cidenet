using Domain.Users;

namespace Api.Contracts.Users;

public record EditUserRequest(string? Nombre, string? Email, Role? Rol, UserStatus? Estado);
