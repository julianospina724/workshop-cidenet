using Domain.Users;

namespace Api.Contracts.Users;

public record UserResponse(Guid Id, string Nombre, string Email, Role Rol, UserStatus Estado, DateTime CreatedAt)
{
    public static UserResponse FromEntity(User user) =>
        new(user.Id, user.Nombre, user.Email, user.Rol, user.Estado, user.CreatedAt);
}
