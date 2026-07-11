using Domain.Permissions;

namespace Application.Permissions;

public class PermissionsService
{
    private readonly IPermissionRepository _repository;

    public PermissionsService(IPermissionRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<PermissionMatrixEntry>> GetMatrixAsync() => _repository.GetAllAsync();

    public async Task<UpdatePermissionsResult> UpdateAsync(UpdatePermissionsCommand command)
    {
        if (command.Changes.Any(c => c.Rol == command.CallerRole))
        {
            return UpdatePermissionsResult.ForbiddenResult("No puedes modificar los permisos de tu propio rol.");
        }

        foreach (var change in command.Changes)
        {
            var entry = await _repository.FindEntryAsync(change.Rol, change.Recurso, change.Accion);
            if (entry is not null)
            {
                entry.Permitido = change.Permitido;
            }
        }

        await _repository.SaveChangesAsync();

        return UpdatePermissionsResult.Success();
    }
}
