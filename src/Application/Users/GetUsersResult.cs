using Domain.Users;

namespace Application.Users;

public class GetUsersResult
{
    public bool Succeeded { get; private init; }
    public string? Error { get; private init; }
    public IReadOnlyList<User> Items { get; private init; } = Array.Empty<User>();
    public int TotalCount { get; private init; }
    public int Page { get; private init; }
    public int PageSize { get; private init; }

    public static GetUsersResult Success(IReadOnlyList<User> items, int totalCount, int page, int pageSize) =>
        new() { Succeeded = true, Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };

    public static GetUsersResult Failure(string error) => new() { Succeeded = false, Error = error };
}
