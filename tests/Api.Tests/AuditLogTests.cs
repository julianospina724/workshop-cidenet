using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Audit;
using Domain.Audit;
using Domain.Permissions;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.Tests;

public class AuditLogTests : IClassFixture<SqliteWebApplicationFactory>
{
    private readonly SqliteWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuditLogTests(SqliteWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private AppDbContext GetDbContext(IServiceScope scope) => scope.ServiceProvider.GetRequiredService<AppDbContext>();

    [Fact]
    public async Task Crear_un_usuario_genera_su_registro_de_auditoria()
    {
        await TestUserFactory.CreateAsync(_client, "audit-crear@mail.com", "Usuario Audit Crear", "Editor");

        using var scope = _factory.Services.CreateScope();
        var db = GetDbContext(scope);
        var log = db.AuditLogs.SingleOrDefault(a =>
            a.EntityName == nameof(User) && a.Action == "create" && a.Details!.Contains("audit-crear@mail.com"));

        Assert.NotNull(log);
        Assert.Contains("Nombre=", log!.Details);
        Assert.NotEqual(Guid.Empty, log.PerformedByUserId);
    }

    [Fact]
    public async Task Editar_un_usuario_genera_su_registro_de_auditoria()
    {
        var adminToken = await TestUserFactory.LoginAsSeededAdminAsync(_client);
        var target = await TestUserFactory.CreateAsync(_client, "audit-editar@mail.com", "Usuario Audit Editar", "Editor");

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{target.Id}")
        {
            Content = JsonContent.Create(new { Nombre = "Usuario Audit Editado" }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.SendAsync(request);

        using var scope = _factory.Services.CreateScope();
        var db = GetDbContext(scope);
        var log = db.AuditLogs.SingleOrDefault(a => a.EntityName == nameof(User) && a.Action == "update" && a.Details!.Contains("Audit Editado"));

        Assert.NotNull(log);
    }

    [Fact]
    public async Task Eliminar_un_usuario_genera_su_registro_de_auditoria_con_accion_delete()
    {
        var adminToken = await TestUserFactory.LoginAsSeededAdminAsync(_client);
        var target = await TestUserFactory.CreateAsync(_client, "audit-eliminar@mail.com", "Usuario Audit Eliminar", "Editor");

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{target.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.SendAsync(request);

        using var scope = _factory.Services.CreateScope();
        var db = GetDbContext(scope);
        var deleteLog = db.AuditLogs
            .Where(a => a.EntityName == nameof(User) && a.Action == "delete")
            .AsEnumerable()
            .SingleOrDefault(a => a.Details!.Contains("Eliminado"));

        Assert.NotNull(deleteLog);
    }

    [Fact]
    public async Task Cambiar_un_permiso_en_la_matriz_genera_su_registro_de_auditoria()
    {
        var adminToken = await TestUserFactory.LoginAsSeededAdminAsync(_client);

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/permissions")
        {
            Content = JsonContent.Create(new
            {
                Changes = new[] { new { Rol = "Viewer", Recurso = "Reports", Accion = "Read", Permitido = false } },
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.SendAsync(request);

        using var scope = _factory.Services.CreateScope();
        var db = GetDbContext(scope);
        var log = db.AuditLogs.SingleOrDefault(a =>
            a.EntityName == nameof(PermissionMatrixEntry) && a.Details!.Contains("Viewer/Reports/Read"));

        Assert.NotNull(log);

        // Restauramos el valor por defecto para no interferir con otros tests de esta clase.
        request = new HttpRequestMessage(HttpMethod.Put, "/api/permissions")
        {
            Content = JsonContent.Create(new
            {
                Changes = new[] { new { Rol = "Viewer", Recurso = "Reports", Accion = "Read", Permitido = true } },
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.SendAsync(request);
    }

    [Fact]
    public async Task Los_registros_de_auditoria_de_mas_de_6_meses_se_purgan()
    {
        await using var isolatedFactory = new SqliteWebApplicationFactory();
        isolatedFactory.Clock.UtcNow = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        using (var scope = isolatedFactory.Services.CreateScope())
        {
            var db = GetDbContext(scope);
            db.AuditLogs.Add(new AuditLog
            {
                PerformedByUserId = Guid.NewGuid(),
                EntityName = "User",
                Action = "create",
                Details = "registro viejo",
                CreatedAt = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            });
            db.AuditLogs.Add(new AuditLog
            {
                PerformedByUserId = Guid.NewGuid(),
                EntityName = "User",
                Action = "create",
                Details = "registro reciente",
                CreatedAt = new DateTime(2026, 6, 20, 0, 0, 0, DateTimeKind.Utc),
            });
            await db.SaveChangesAsync();
        }

        using (var scope = isolatedFactory.Services.CreateScope())
        {
            var retentionService = scope.ServiceProvider.GetRequiredService<AuditRetentionService>();
            await retentionService.PurgeOldEntriesAsync();
        }

        using (var scope = isolatedFactory.Services.CreateScope())
        {
            var db = GetDbContext(scope);
            Assert.DoesNotContain(db.AuditLogs, a => a.Details == "registro viejo");
            Assert.Contains(db.AuditLogs, a => a.Details == "registro reciente");
        }
    }
}
