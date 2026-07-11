namespace Application.Users;

public class DeleteUserResult
{
    public bool Succeeded { get; private init; }
    public bool NotFound { get; private init; }
    public string? Message { get; private init; }

    public static DeleteUserResult Success() => new() { Succeeded = true };

    public static DeleteUserResult NotFoundResult() => new() { Succeeded = false, NotFound = true };

    public static DeleteUserResult Failure(string message) => new() { Succeeded = false, Message = message };
}
