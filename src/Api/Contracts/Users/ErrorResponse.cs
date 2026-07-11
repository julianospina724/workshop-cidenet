namespace Api.Contracts.Users;

public record ErrorResponse(string? Message, IReadOnlyDictionary<string, string>? FieldErrors);
