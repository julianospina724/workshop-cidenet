using Domain.Users;

namespace Application.Users;

public class AuthenticateResult
{
    public bool Succeeded { get; private init; }
    public bool LockedOut { get; private init; }
    public User? User { get; private init; }

    public static AuthenticateResult Success(User user) => new() { Succeeded = true, User = user };

    public static AuthenticateResult InvalidCredentials() => new() { Succeeded = false };

    public static AuthenticateResult LockedOutResult() => new() { Succeeded = false, LockedOut = true };
}
