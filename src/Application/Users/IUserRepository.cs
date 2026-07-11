using Domain.Users;

namespace Application.Users;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string normalizedEmail);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
