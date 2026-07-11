using Domain.Users;

namespace Application.Users;

public class EditUserService
{
    private readonly IUserRepository _repository;

    public EditUserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<EditUserResult> EditAsync(Guid callerUserId, EditUserCommand command)
    {
        var target = await _repository.FindByIdAsync(command.TargetUserId);
        if (target is null)
        {
            return EditUserResult.NotFoundResult();
        }

        var isSelf = target.Id == callerUserId;

        if (isSelf && command.Rol.HasValue && command.Rol.Value != target.Rol)
        {
            return EditUserResult.ForbiddenResult("No puedes modificar tu propio rol.");
        }

        var isOnlyActiveAdmin = target.Rol == Role.Admin
            && target.Estado == UserStatus.Activo
            && await _repository.CountActiveAdminsAsync() <= 1;

        if (isOnlyActiveAdmin)
        {
            if (command.Rol.HasValue && command.Rol.Value != Role.Admin)
            {
                return EditUserResult.Failure("Debe existir al menos un Admin activo en el sistema.");
            }

            if (command.Estado.HasValue && command.Estado.Value != UserStatus.Activo)
            {
                return EditUserResult.Failure("Debe existir al menos un Admin activo en el sistema.");
            }
        }

        var fieldErrors = new Dictionary<string, string>();
        string? normalizedNombre = null;
        if (command.Nombre is not null)
        {
            normalizedNombre = TextNormalizer.NormalizeName(command.Nombre);
            if (string.IsNullOrWhiteSpace(normalizedNombre))
            {
                fieldErrors["nombre"] = "El nombre es obligatorio.";
            }
        }

        string? normalizedEmail = null;
        if (command.Email is not null)
        {
            normalizedEmail = TextNormalizer.NormalizeEmail(command.Email);
            if (!EmailFormat.IsValid(normalizedEmail))
            {
                fieldErrors["email"] = "El email no tiene un formato válido.";
            }
        }

        if (fieldErrors.Count > 0)
        {
            return EditUserResult.ValidationFailure(fieldErrors);
        }

        if (normalizedEmail is not null && normalizedEmail != target.Email && await _repository.EmailExistsAsync(normalizedEmail))
        {
            return EditUserResult.Failure("El correo ya está registrado.");
        }

        if (normalizedNombre is not null)
        {
            target.Nombre = normalizedNombre;
        }

        if (normalizedEmail is not null)
        {
            target.Email = normalizedEmail;
        }

        if (command.Rol.HasValue)
        {
            target.Rol = command.Rol.Value;
        }

        if (command.Estado.HasValue)
        {
            target.Estado = command.Estado.Value;
        }

        try
        {
            await _repository.SaveChangesAsync();
        }
        catch (EmailAlreadyRegisteredException ex)
        {
            return EditUserResult.Failure(ex.Message);
        }

        return EditUserResult.Success(target);
    }
}
