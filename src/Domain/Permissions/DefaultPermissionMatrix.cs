using Domain.Users;

namespace Domain.Permissions;

/// <summary>
/// Estado por defecto de la matriz de permisos, tal como lo define el caso
/// (brief §4). Se usa como datos semilla (EF Core HasData) — Ids y timestamps
/// fijos para que la migración sea determinista.
/// </summary>
public static class DefaultPermissionMatrix
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static IReadOnlyList<PermissionMatrixEntry> Entries { get; } = Build();

    private static IReadOnlyList<PermissionMatrixEntry> Build()
    {
        // Crud = [Create, Read, Update, Delete], según la matriz del caso.
        var matrix = new (Role Rol, Resource Recurso, bool[] Crud)[]
        {
            (Role.Admin, Resource.Users, new[] { true, true, true, true }),
            (Role.Admin, Resource.Roles, new[] { true, true, true, true }),
            (Role.Admin, Resource.Permissions, new[] { true, true, true, true }),
            (Role.Admin, Resource.Reports, new[] { true, true, true, true }),
            (Role.Editor, Resource.Users, new[] { false, true, false, false }),
            (Role.Editor, Resource.Roles, new[] { false, true, false, false }),
            (Role.Editor, Resource.Permissions, new[] { false, true, false, false }),
            (Role.Editor, Resource.Reports, new[] { true, true, true, true }),
            (Role.Viewer, Resource.Users, new[] { false, false, false, false }),
            (Role.Viewer, Resource.Roles, new[] { false, false, false, false }),
            (Role.Viewer, Resource.Permissions, new[] { false, false, false, false }),
            (Role.Viewer, Resource.Reports, new[] { false, true, false, false }),
        };

        var actions = new[] { PermissionAction.Create, PermissionAction.Read, PermissionAction.Update, PermissionAction.Delete };
        var entries = new List<PermissionMatrixEntry>();
        var counter = 1;

        foreach (var (rol, recurso, crud) in matrix)
        {
            for (var i = 0; i < actions.Length; i++)
            {
                entries.Add(new PermissionMatrixEntry
                {
                    Id = Guid.Parse($"00000000-0000-0000-0000-{counter:D12}"),
                    Rol = rol,
                    Recurso = recurso,
                    Accion = actions[i],
                    Permitido = crud[i],
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = SeedTimestamp,
                });
                counter++;
            }
        }

        return entries;
    }
}
