using Domain.Users;

namespace Application.Users;

public class DeleteUserService
{
    private readonly IUserRepository _repository;

    public DeleteUserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeleteUserResult> DeleteAsync(Guid targetUserId)
    {
        var target = await _repository.FindByIdAsync(targetUserId);
        if (target is null || target.Estado == UserStatus.Eliminado)
        {
            return DeleteUserResult.NotFoundResult();
        }

        var isOnlyActiveAdmin = target.Rol == Role.Admin
            && target.Estado == UserStatus.Activo
            && await _repository.CountActiveAdminsAsync() <= 1;

        if (isOnlyActiveAdmin)
        {
            return DeleteUserResult.Failure("Debe existir al menos un Admin activo en el sistema.");
        }

        target.Estado = UserStatus.Eliminado;
        await _repository.SaveChangesAsync();

        return DeleteUserResult.Success();
    }
}
