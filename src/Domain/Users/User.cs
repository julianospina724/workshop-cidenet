using Domain.Common;

namespace Domain.Users;

public class User : Entity
{
    public required string Nombre { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public Role Rol { get; set; }
    public UserStatus Estado { get; set; } = UserStatus.Activo;
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntilUtc { get; set; }

    public static User Create(string nombre, string email, string passwordHash, Role rol)
    {
        var normalizedNombre = TextNormalizer.NormalizeName(nombre);
        var normalizedEmail = TextNormalizer.NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(normalizedNombre))
        {
            throw new DomainException("El nombre es obligatorio.");
        }

        if (!EmailFormat.IsValid(normalizedEmail))
        {
            throw new DomainException("El email no tiene un formato válido.");
        }

        return new User
        {
            Nombre = normalizedNombre,
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Rol = rol,
            Estado = UserStatus.Activo,
        };
    }

    public bool PuedeAutenticarse => Estado == UserStatus.Activo;

    public bool IsLockedOut(DateTime nowUtc) => LockedUntilUtc.HasValue && LockedUntilUtc.Value > nowUtc;

    public void ClearLockoutIfExpired(DateTime nowUtc)
    {
        if (LockedUntilUtc.HasValue && LockedUntilUtc.Value <= nowUtc)
        {
            FailedLoginAttempts = 0;
            LockedUntilUtc = null;
        }
    }

    public void RegisterFailedLoginAttempt(DateTime nowUtc)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= LoginLockoutPolicy.MaxFailedAttempts)
        {
            LockedUntilUtc = nowUtc.Add(LoginLockoutPolicy.LockoutDuration);
        }
    }

    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockedUntilUtc = null;
    }
}
