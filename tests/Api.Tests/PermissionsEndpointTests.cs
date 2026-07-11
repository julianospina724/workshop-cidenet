using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Contracts.Auth;
using Api.Contracts.Permissions;
using Api.Contracts.Users;
using Application.Permissions;
using Domain.Permissions;
using Domain.Users;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.Tests;

public class PermissionsEndpointTests : IClassFixture<SqliteWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SqliteWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PermissionsEndpointTests(SqliteWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> CreateUserAndLoginAsync(string email, string nombre, string rol)
    {
        var (_, token) = await TestUserFactory.CreateAndLoginAsync(_client, email, nombre, rol);
        return token;
    }

    private HttpRequestMessage AuthorizedGet(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private HttpRequestMessage AuthorizedPut(string url, string token, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    [Fact]
    public async Task Admin_consulta_la_matriz_con_el_estado_por_defecto_del_caso()
    {
        var token = await CreateUserAndLoginAsync("permisos-consulta-admin@mail.com", "Admin Consulta", "Admin");

        var response = await _client.SendAsync(AuthorizedGet("/api/permissions", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PermissionMatrixResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(48, body!.Entries.Count);

        bool Permitido(Role rol, Resource recurso, PermissionAction accion) =>
            body.Entries.Single(e => e.Rol == rol && e.Recurso == recurso && e.Accion == accion).Permitido;

        Assert.True(Permitido(Role.Admin, Resource.Users, PermissionAction.Delete));
        Assert.True(Permitido(Role.Editor, Resource.Users, PermissionAction.Read));
        Assert.False(Permitido(Role.Editor, Resource.Users, PermissionAction.Delete));
        Assert.False(Permitido(Role.Viewer, Resource.Users, PermissionAction.Read));
        Assert.True(Permitido(Role.Editor, Resource.Reports, PermissionAction.Create));
        Assert.True(Permitido(Role.Viewer, Resource.Reports, PermissionAction.Read));
        Assert.False(Permitido(Role.Viewer, Resource.Reports, PermissionAction.Create));
    }

    [Fact]
    public async Task Admin_activa_y_desactiva_un_permiso_y_guarda()
    {
        var token = await CreateUserAndLoginAsync("permisos-cambiar-admin@mail.com", "Admin Cambiar", "Admin");

        var putResponse = await _client.SendAsync(AuthorizedPut("/api/permissions", token, new
        {
            Changes = new[] { new { Rol = "Editor", Recurso = "Reports", Accion = "Create", Permitido = false } },
        }));

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var getResponse = await _client.SendAsync(AuthorizedGet("/api/permissions", token));
        var body = await getResponse.Content.ReadFromJsonAsync<PermissionMatrixResponse>(JsonOptions);
        var entry = body!.Entries.Single(e => e.Rol == Role.Editor && e.Recurso == Resource.Reports && e.Accion == PermissionAction.Create);

        Assert.False(entry.Permitido);
    }

    [Fact]
    public async Task No_puedo_modificar_los_permisos_de_mi_propio_rol()
    {
        var token = await CreateUserAndLoginAsync("permisos-propiorol-admin@mail.com", "Admin Propio Rol Permisos", "Admin");

        var response = await _client.SendAsync(AuthorizedPut("/api/permissions", token, new
        {
            Changes = new[] { new { Rol = "Admin", Recurso = "Users", Accion = "Delete", Permitido = false } },
        }));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var getResponse = await _client.SendAsync(AuthorizedGet("/api/permissions", token));
        var body = await getResponse.Content.ReadFromJsonAsync<PermissionMatrixResponse>(JsonOptions);
        var entry = body!.Entries.Single(e => e.Rol == Role.Admin && e.Recurso == Resource.Users && e.Accion == PermissionAction.Delete);

        Assert.True(entry.Permitido);
    }

    [Fact]
    public async Task Solo_admin_puede_consultar_la_matriz()
    {
        var token = await CreateUserAndLoginAsync("permisos-solo-admin-get-editor@mail.com", "Editor Consulta Permisos", "Editor");

        var response = await _client.SendAsync(AuthorizedGet("/api/permissions", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Solo_admin_puede_modificar_la_matriz()
    {
        var token = await CreateUserAndLoginAsync("permisos-solo-admin-put-editor@mail.com", "Editor Modifica Permisos", "Editor");

        var response = await _client.SendAsync(AuthorizedPut("/api/permissions", token, new
        {
            Changes = new[] { new { Rol = "Viewer", Recurso = "Reports", Accion = "Read", Permitido = false } },
        }));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Fallo_general_al_guardar_la_matriz_devuelve_500()
    {
        var token = await CreateUserAndLoginAsync("permisos-fallo-admin@mail.com", "Admin Fallo Permisos", "Admin");

        await using var brokenFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPermissionRepository));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddScoped<IPermissionRepository, ThrowingPermissionRepository>();
            });
        });

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/permissions")
        {
            Content = JsonContent.Create(new
            {
                Changes = new[] { new { Rol = "Viewer", Recurso = "Reports", Accion = "Read", Permitido = false } },
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await brokenFactory.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private class ThrowingPermissionRepository : IPermissionRepository
    {
        public Task<IReadOnlyList<PermissionMatrixEntry>> GetAllAsync() =>
            throw new InvalidOperationException("Fallo simulado para el test.");

        public Task<PermissionMatrixEntry?> FindEntryAsync(Role rol, Resource recurso, PermissionAction accion) =>
            throw new InvalidOperationException("Fallo simulado para el test.");

        public Task SaveChangesAsync() => throw new InvalidOperationException("Fallo simulado para el test.");
    }
}
