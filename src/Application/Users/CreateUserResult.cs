using Domain.Users;

namespace Application.Users;

public class CreateUserResult
{
    public bool Succeeded { get; private init; }
    public User? User { get; private init; }
    public string? Message { get; private init; }
    public IReadOnlyDictionary<string, string>? FieldErrors { get; private init; }

    public static CreateUserResult Success(User user) => new() { Succeeded = true, User = user };

    public static CreateUserResult ValidationFailure(IReadOnlyDictionary<string, string> fieldErrors) =>
        new() { Succeeded = false, FieldErrors = fieldErrors };

    public static CreateUserResult Failure(string message) => new() { Succeeded = false, Message = message };
}
