using Application.Common;
using Domain.Users;

namespace Application.Users;

public class AuthenticateService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthenticateService(IUserRepository repository, IPasswordHasher hasher, IClock clock, ITokenGenerator tokenGenerator)
    {
        _repository = repository;
        _hasher = hasher;
        _clock = clock;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthenticateResult> AuthenticateAsync(AuthenticateCommand command)
    {
        var normalizedEmail = TextNormalizer.NormalizeEmail(command.Email);
        var user = await _repository.FindByNormalizedEmailAsync(normalizedEmail);

        if (user is null)
        {
            return AuthenticateResult.InvalidCredentials();
        }

        var now = _clock.UtcNow;
        user.ClearLockoutIfExpired(now);

        if (user.IsLockedOut(now))
        {
            return AuthenticateResult.LockedOutResult();
        }

        if (!user.PuedeAutenticarse || !_hasher.Verify(command.Password, user.PasswordHash))
        {
            if (user.PuedeAutenticarse)
            {
                user.RegisterFailedLoginAttempt(now);
            }

            await _repository.SaveChangesAsync();
            return AuthenticateResult.InvalidCredentials();
        }

        user.RegisterSuccessfulLogin();
        await _repository.SaveChangesAsync();

        var token = _tokenGenerator.GenerateToken(user);
        return AuthenticateResult.Success(user, token);
    }
}
