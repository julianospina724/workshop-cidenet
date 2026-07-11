using Domain.Audit;
using Domain.Permissions;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Api.Tests;

public class DomainPersistenceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;

    public DomainPersistenceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Persiste_y_recupera_un_usuario()
    {
        var user = new User
        {
            Nombre = "Ana Pérez",
            Email = "ana.perez@mail.com",
            PasswordHash = "hash-no-reversible",
            Rol = Role.Editor,
            Estado = UserStatus.Activo,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var found = await _context.Users.FindAsync(user.Id);

        Assert.NotNull(found);
        Assert.Equal("ana.perez@mail.com", found!.Email);
        Assert.Equal(Role.Editor, found.Rol);
        Assert.Equal(UserStatus.Activo, found.Estado);
    }

    [Fact]
    public async Task Email_duplicado_viola_la_restriccion_unica()
    {
        _context.Users.Add(new User
        {
            Nombre = "Ana Pérez",
            Email = "ana.perez@mail.com",
            PasswordHash = "hash-1",
            Rol = Role.Viewer,
            Estado = UserStatus.Activo,
        });
        await _context.SaveChangesAsync();

        _context.Users.Add(new User
        {
            Nombre = "Otra Persona",
            Email = "ana.perez@mail.com",
            PasswordHash = "hash-2",
            Rol = Role.Viewer,
            Estado = UserStatus.Activo,
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task Persiste_y_recupera_una_entrada_de_la_matriz_de_permisos()
    {
        var entry = new PermissionMatrixEntry
        {
            Rol = Role.Admin,
            Recurso = Resource.Users,
            Accion = PermissionAction.Delete,
            Permitido = true,
        };

        _context.PermissionMatrixEntries.Add(entry);
        await _context.SaveChangesAsync();

        var found = await _context.PermissionMatrixEntries.FindAsync(entry.Id);

        Assert.NotNull(found);
        Assert.Equal(Role.Admin, found!.Rol);
        Assert.Equal(Resource.Users, found.Recurso);
        Assert.Equal(PermissionAction.Delete, found.Accion);
        Assert.True(found.Permitido);
    }

    [Fact]
    public async Task Persiste_y_recupera_un_registro_de_auditoria()
    {
        var log = new AuditLog
        {
            PerformedByUserId = Guid.NewGuid(),
            EntityName = "User",
            Action = "create",
            Details = "{\"email\":\"ana.perez@mail.com\"}",
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();

        var found = await _context.AuditLogs.FindAsync(log.Id);

        Assert.NotNull(found);
        Assert.Equal("User", found!.EntityName);
        Assert.Equal("create", found.Action);
    }
}
