using Domain.Users;

namespace Application.Users;

public class CreateUserService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _hasher;

    public CreateUserService(IUserRepository repository, IPasswordHasher hasher)
    {
        _repository = repository;
        _hasher = hasher;
    }

    public async Task<CreateUserResult> CreateAsync(CreateUserCommand command)
    {
        var fieldErrors = new Dictionary<string, string>();

        var normalizedNombre = TextNormalizer.NormalizeName(command.Nombre);
        if (string.IsNullOrWhiteSpace(normalizedNombre))
        {
            fieldErrors["nombre"] = "El nombre es obligatorio.";
        }

        var normalizedEmail = TextNormalizer.NormalizeEmail(command.Email);
        if (!EmailFormat.IsValid(normalizedEmail))
        {
            fieldErrors["email"] = "El email no tiene un formato válido.";
        }

        if (!PasswordPolicy.IsValid(command.Password))
        {
            fieldErrors["password"] = "La contraseña debe tener mínimo 8 caracteres, con mayúscula, minúscula, número y símbolo ($&*#@).";
        }
        else if (command.Password != command.ConfirmPassword)
        {
            fieldErrors["confirmPassword"] = "La contraseña y su confirmación no coinciden.";
        }

        if (fieldErrors.Count > 0)
        {
            return CreateUserResult.ValidationFailure(fieldErrors);
        }

        if (await _repository.EmailExistsAsync(normalizedEmail))
        {
            return CreateUserResult.Failure("El correo ya está registrado.");
        }

        var passwordHash = _hasher.Hash(command.Password);
        var user = User.Create(command.Nombre, command.Email, passwordHash, command.Rol);

        await _repository.AddAsync(user);

        try
        {
            await _repository.SaveChangesAsync();
        }
        catch (EmailAlreadyRegisteredException ex)
        {
            return CreateUserResult.Failure(ex.Message);
        }

        return CreateUserResult.Success(user);
    }
}
