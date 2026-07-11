using Application.Common;

namespace Infrastructure.Common;

/// <summary>
/// Usado cuando no hay una petición HTTP en curso (migraciones, herramientas
/// de diseño de EF Core). No hay "quién" al que atribuir el cambio.
/// </summary>
public class NullCurrentUserAccessor : ICurrentUserAccessor
{
    public Guid? CurrentUserId => null;
}
