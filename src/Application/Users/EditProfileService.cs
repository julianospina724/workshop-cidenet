using Domain.Users;

namespace Application.Users;

/// <summary>
/// Autoservicio de perfil: cualquier usuario edita únicamente su propio
/// registro (nombre, email). A propósito no acepta Rol ni Estado en su
/// comando — no hay campo que manipular para escalar privilegios porque
/// el contrato del endpoint nunca los expone.
/// </summary>
public class EditProfileService
{
    private readonly IUserRepository _repository;

    public EditProfileService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<EditProfileResult> EditAsync(EditProfileCommand command)
    {
        var user = await _repository.FindByIdAsync(command.CallerId);
        if (user is null || user.Estado != UserStatus.Activo)
        {
            return EditProfileResult.Failure("Tu cuenta ya no está activa.");
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
            return EditProfileResult.ValidationFailure(fieldErrors);
        }

        if (normalizedEmail is not null && normalizedEmail != user.Email && await _repository.EmailExistsAsync(normalizedEmail))
        {
            return EditProfileResult.Failure("El correo ya está registrado.");
        }

        if (normalizedNombre is not null)
        {
            user.Nombre = normalizedNombre;
        }

        if (normalizedEmail is not null)
        {
            user.Email = normalizedEmail;
        }

        try
        {
            await _repository.SaveChangesAsync();
        }
        catch (EmailAlreadyRegisteredException ex)
        {
            return EditProfileResult.Failure(ex.Message);
        }

        return EditProfileResult.Success(user);
    }
}
