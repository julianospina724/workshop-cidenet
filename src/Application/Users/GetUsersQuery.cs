using Domain.Users;

namespace Application.Users;

public record GetUsersQuery(
    Role? Rol,
    string? Nombre,
    string? Email,
    UserStatus? Estado,
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    int Page,
    int PageSize);
