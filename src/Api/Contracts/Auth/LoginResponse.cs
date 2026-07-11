using Api.Contracts.Users;

namespace Api.Contracts.Auth;

public record LoginResponse(string Token, UserResponse User);
