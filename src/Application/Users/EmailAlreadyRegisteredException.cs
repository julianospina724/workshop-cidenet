namespace Application.Users;

/// <summary>
/// Lanzada por la implementación de <see cref="IUserRepository"/> cuando la
/// restricción única de email en base de datos rechaza el guardado — el
/// respaldo ante condiciones de carrera que la validación de aplicación no cubre.
/// </summary>
public class EmailAlreadyRegisteredException : Exception
{
    public EmailAlreadyRegisteredException(Exception innerException)
        : base("El correo ya está registrado.", innerException)
    {
    }
}
