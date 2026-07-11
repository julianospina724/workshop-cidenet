using Domain.Users;

namespace Application.Users;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string normalizedEmail);
    Task<User?> FindByNormalizedEmailAsync(string normalizedEmail);
    Task<(IReadOnlyList<User> Items, int TotalCount)> SearchAsync(GetUsersQuery query);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
