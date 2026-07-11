using System.Security.Cryptography;
using Application.Users;

namespace Infrastructure.Security;

/// <summary>
/// PBKDF2 con sal aleatoria por contraseña. El hash resultante es unidireccional:
/// nunca se puede recuperar la contraseña original a partir de él (ver gap-SEC:
/// la contraseña nunca viaja de vuelta en ninguna respuesta del sistema).
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const int Iterations = 100_000;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSizeBytes);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }
}
