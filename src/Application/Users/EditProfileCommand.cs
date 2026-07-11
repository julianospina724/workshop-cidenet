namespace Application.Users;

public record EditProfileCommand(Guid CallerId, string? Nombre, string? Email);
