using Domain.Common;
using Domain.Users;

namespace Domain.Permissions;

public class PermissionMatrixEntry : Entity
{
    public Role Rol { get; set; }
    public Resource Recurso { get; set; }
    public PermissionAction Accion { get; set; }
    public bool Permitido { get; set; }
}
