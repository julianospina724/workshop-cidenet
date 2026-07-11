using Domain.Common;

namespace Domain.Users;

public class User : Entity
{
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public Role Rol { get; set; }
    public UserStatus Estado { get; set; } = UserStatus.Activo;
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
}
