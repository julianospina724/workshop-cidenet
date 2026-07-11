using Domain.Users;

namespace Application.Users;

public class EditUserResult
{
    public bool Succeeded { get; private init; }
    public bool NotFound { get; private init; }
    public bool Forbidden { get; private init; }
    public User? User { get; private init; }
    public string? Message { get; private init; }
    public IReadOnlyDictionary<string, string>? FieldErrors { get; private init; }

    public static EditUserResult Success(User user) => new() { Succeeded = true, User = user };

    public static EditUserResult NotFoundResult() => new() { Succeeded = false, NotFound = true };

    public static EditUserResult ForbiddenResult(string message) =>
        new() { Succeeded = false, Forbidden = true, Message = message };

    public static EditUserResult ValidationFailure(IReadOnlyDictionary<string, string> fieldErrors) =>
        new() { Succeeded = false, FieldErrors = fieldErrors };

    public static EditUserResult Failure(string message) => new() { Succeeded = false, Message = message };
}
