namespace Api.Contracts.Users;

public record PagedUsersResponse(IReadOnlyList<UserResponse> Items, int TotalCount, int Page, int PageSize);
