using Application.Users;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<bool> EmailExistsAsync(string normalizedEmail) =>
        _db.Users.AnyAsync(u => u.Email == normalizedEmail);

    public async Task AddAsync(User user) => await _db.Users.AddAsync(user);

    public async Task SaveChangesAsync()
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new EmailAlreadyRegisteredException(ex);
        }
    }
}
