namespace Api.Contracts.Users;

/// <summary>
/// A propósito no incluye Rol ni Estado — el autoservicio de perfil nunca
/// puede tocarlos, ni siquiera si el cliente los envía en el JSON (System.Text.Json
/// ignora silenciosamente cualquier propiedad no mapeada).
/// </summary>
public record EditProfileRequest(string? Nombre, string? Email);
