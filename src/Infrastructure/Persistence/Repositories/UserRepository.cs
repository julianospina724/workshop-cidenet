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

    public Task<User?> FindByNormalizedEmailAsync(string normalizedEmail) =>
        _db.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail);

    public Task<User?> FindByIdAsync(Guid id) => _db.Users.FindAsync(id).AsTask();

    public Task<int> CountActiveAdminsAsync() =>
        _db.Users.CountAsync(u => u.Rol == Role.Admin && u.Estado == UserStatus.Activo);

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> SearchAsync(GetUsersQuery query)
    {
        var q = _db.Users.Where(u => u.Estado != UserStatus.Eliminado);

        if (query.Rol.HasValue)
        {
            q = q.Where(u => u.Rol == query.Rol.Value);
        }

        if (query.Estado.HasValue)
        {
            q = q.Where(u => u.Estado == query.Estado.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Nombre))
        {
            var nombre = query.Nombre.ToLowerInvariant();
            q = q.Where(u => u.Nombre.ToLower().Contains(nombre));
        }

        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            var email = query.Email.ToLowerInvariant();
            q = q.Where(u => u.Email.Contains(email));
        }

        if (query.FechaDesde.HasValue)
        {
            q = q.Where(u => u.CreatedAt >= query.FechaDesde.Value);
        }

        if (query.FechaHasta.HasValue)
        {
            q = q.Where(u => u.CreatedAt <= query.FechaHasta.Value);
        }

        var total = await q.CountAsync();
        var items = await q.OrderBy(u => u.Nombre)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, total);
    }

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
