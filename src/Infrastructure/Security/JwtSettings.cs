namespace Infrastructure.Security;

/// <summary>
/// Configuración compartida entre la emisión (JwtTokenGenerator) y la
/// validación (middleware de JWT Bearer en Program.cs) del token.
/// </summary>
public static class JwtSettings
{
    public const string Issuer = "workshop-cidenet";
    public const string Audience = "workshop-cidenet-clients";
    public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

    /// <summary>
    /// En producción esto debe venir de una variable de entorno/secreto real
    /// (JWT_SIGNING_KEY). El valor por defecto es solo para desarrollo/tests.
    /// </summary>
    public static string SigningKey =>
        Environment.GetEnvironmentVariable("JWT_SIGNING_KEY")
        ?? "dev-only-signing-key-change-in-production-32-bytes-min";
}
