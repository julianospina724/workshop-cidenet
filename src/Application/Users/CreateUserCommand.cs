using Domain.Users;

namespace Application.Users;

public record CreateUserCommand(string Nombre, string Email, string Password, string ConfirmPassword, Role Rol);
