namespace Domain.Users;

/// <summary>
/// Admin inicial sembrado vía migración (EF Core HasData) — resuelve el
/// problema del huevo y la gallina: sin este usuario, nadie podría crear la
/// primera cuenta una vez que POST /api/users pasó a exigir rol Admin.
/// Contraseña: "Admin123$" (documentada para el primer login; cámbiala tras
/// el primer arranque en un entorno real — el hash aquí usa una sal fija,
/// válida solo como semilla de desarrollo).
/// </summary>
public static class DefaultAdminSeed
{
    private static readonly Guid Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public const string Email = "admin@workshop-cidenet.local";
    public const string Password = "Admin123$";
    private const string PasswordHash = "100000.AAAAAAAAAAAAAAAAAAAAAA==.7/Lw7TTMWGMGUNZ+CTTZiAjdfwJlJs6x9+mANRDlZu0=";

    public static User Entry { get; } = new()
    {
        Id = Id,
        Nombre = "Admin Inicial",
        Email = Email,
        PasswordHash = PasswordHash,
        Rol = Role.Admin,
        Estado = UserStatus.Activo,
        CreatedAt = SeedTimestamp,
        UpdatedAt = SeedTimestamp,
    };
}
