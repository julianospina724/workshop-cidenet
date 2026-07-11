namespace Application.Permissions;

public class UpdatePermissionsResult
{
    public bool Succeeded { get; private init; }
    public bool Forbidden { get; private init; }
    public string? Message { get; private init; }

    public static UpdatePermissionsResult Success() => new() { Succeeded = true };

    public static UpdatePermissionsResult ForbiddenResult(string message) =>
        new() { Succeeded = false, Forbidden = true, Message = message };
}
