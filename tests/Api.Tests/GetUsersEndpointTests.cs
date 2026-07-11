using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Contracts.Auth;
using Api.Contracts.Users;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.Tests;

public class GetUsersEndpointTests : IClassFixture<SqliteWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SqliteWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetUsersEndpointTests(SqliteWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> CreateUserAndLoginAsync(string email, string nombre = "Usuario Prueba", string rol = "Admin")
    {
        await _client.PostAsJsonAsync("/api/users", new
        {
            Nombre = nombre,
            Email = email,
            Password = "Clave123$",
            ConfirmPassword = "Clave123$",
            Rol = rol,
        });

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Clave123$" });
        var body = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        return body!.Token;
    }

    private HttpRequestMessage AuthorizedGet(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    [Fact]
    public async Task Admin_puede_ver_la_tabla_con_orden_por_defecto()
    {
        var adminToken = await CreateUserAndLoginAsync("tabla-admin@mail.com", "Zoe Última", "Admin");
        await CreateUserAndLoginAsync("tabla-otro@mail.com", "Ana Primera", "Editor");

        var response = await _client.SendAsync(AuthorizedGet("/api/users", adminToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedUsersResponse>(JsonOptions);
        Assert.NotNull(result);
        var nombres = result!.Items.Select(u => u.Nombre).ToList();
        var ordenados = nombres.OrderBy(n => n, StringComparer.Ordinal).ToList();
        Assert.Equal(ordenados, nombres);
    }

    [Fact]
    public async Task Editor_puede_ver_la_tabla()
    {
        var editorToken = await CreateUserAndLoginAsync("tabla-editor@mail.com", "Editor Uno", "Editor");

        var response = await _client.SendAsync(AuthorizedGet("/api/users", editorToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_no_puede_ver_la_tabla()
    {
        var viewerToken = await CreateUserAndLoginAsync("tabla-viewer@mail.com", "Viewer Uno", "Viewer");

        var response = await _client.SendAsync(AuthorizedGet("/api/users", viewerToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Sin_token_devuelve_401()
    {
        var response = await _client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Combinar_filtro_de_rol_y_busqueda_de_nombre()
    {
        var adminToken = await CreateUserAndLoginAsync("combinar-admin@mail.com", "Admin Combinar", "Admin");
        await CreateUserAndLoginAsync("combinar-editor-xyzalfa@mail.com", "Xyzalfa Combinado", "Editor");
        await CreateUserAndLoginAsync("combinar-viewer-xyzalfa@mail.com", "Xyzalfa Viewer", "Viewer");
        await CreateUserAndLoginAsync("combinar-editor-otro@mail.com", "Pedro Otro", "Editor");

        var response = await _client.SendAsync(AuthorizedGet("/api/users?rol=Editor&nombre=xyzalfa", adminToken));

        var result = await response.Content.ReadFromJsonAsync<PagedUsersResponse>(JsonOptions);
        Assert.NotNull(result);
        Assert.Single(result!.Items);
        Assert.Equal("Xyzalfa Combinado", result.Items[0].Nombre);
    }

    [Theory]
    [InlineData("juan")]
    [InlineData("JUAN")]
    public async Task Busqueda_parcial_insensible_a_mayusculas(string textoBuscado)
    {
        var adminToken = await CreateUserAndLoginAsync("busqueda-admin@mail.com", "Admin Busqueda", "Admin");
        await CreateUserAndLoginAsync("busqueda-juan-perez@mail.com", "Juan Pérez", "Editor");

        var response = await _client.SendAsync(AuthorizedGet($"/api/users?nombre={textoBuscado}", adminToken));

        var result = await response.Content.ReadFromJsonAsync<PagedUsersResponse>(JsonOptions);
        Assert.Contains(result!.Items, u => u.Nombre == "Juan Pérez");
    }

    [Fact]
    public async Task Filtrar_por_estado()
    {
        var adminToken = await CreateUserAndLoginAsync("estado-admin@mail.com", "Admin Estado", "Admin");
        await CreateUserAndLoginAsync("estado-inactivo@mail.com", "Usuario Inactivo", "Editor");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = db.Users.Single(u => u.Email == "estado-inactivo@mail.com");
            user.Estado = UserStatus.Inactivo;
            await db.SaveChangesAsync();
        }

        var response = await _client.SendAsync(AuthorizedGet("/api/users?estado=Inactivo", adminToken));

        var result = await response.Content.ReadFromJsonAsync<PagedUsersResponse>(JsonOptions);
        Assert.All(result!.Items, u => Assert.Equal(UserStatus.Inactivo, u.Estado));
        Assert.Contains(result.Items, u => u.Nombre == "Usuario Inactivo");
    }

    [Fact]
    public async Task Rango_de_fechas_invertido_es_rechazado()
    {
        var adminToken = await CreateUserAndLoginAsync("fechas-admin@mail.com", "Admin Fechas", "Admin");

        var desde = DateTime.UtcNow.ToString("O");
        var hasta = DateTime.UtcNow.AddDays(-10).ToString("O");

        var response = await _client.SendAsync(AuthorizedGet($"/api/users?fechaDesde={desde}&fechaHasta={hasta}", adminToken));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Busqueda_sin_resultados()
    {
        var adminToken = await CreateUserAndLoginAsync("sinresultados-admin@mail.com", "Admin Sin Resultados", "Admin");

        var response = await _client.SendAsync(AuthorizedGet("/api/users?nombre=nombre-que-no-existe-en-ningun-lado", adminToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedUsersResponse>(JsonOptions);
        Assert.Empty(result!.Items);
    }

    [Fact]
    public async Task La_tabla_nunca_expone_la_contrasena()
    {
        var adminToken = await CreateUserAndLoginAsync("confidencial-admin@mail.com", "Admin Confidencial", "Admin");

        var response = await _client.SendAsync(AuthorizedGet("/api/users", adminToken));
        var raw = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("password", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Los_usuarios_eliminados_nunca_aparecen()
    {
        var adminToken = await CreateUserAndLoginAsync("eliminados-admin@mail.com", "Admin Eliminados", "Admin");
        await CreateUserAndLoginAsync("eliminados-carlos@mail.com", "Carlos Ruiz", "Editor");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = db.Users.Single(u => u.Email == "eliminados-carlos@mail.com");
            user.Estado = UserStatus.Eliminado;
            await db.SaveChangesAsync();
        }

        var response = await _client.SendAsync(AuthorizedGet("/api/users", adminToken));
        var result = await response.Content.ReadFromJsonAsync<PagedUsersResponse>(JsonOptions);

        Assert.DoesNotContain(result!.Items, u => u.Nombre == "Carlos Ruiz");
    }

    [Fact]
    public async Task Paginacion_respeta_el_tamano_elegido()
    {
        var adminToken = await CreateUserAndLoginAsync("paginacion-admin@mail.com", "Admin Paginacion", "Admin");
        for (var i = 0; i < 5; i++)
        {
            await CreateUserAndLoginAsync($"paginacion-{i}@mail.com", $"Usuario Paginado {i}", "Editor");
        }

        var response = await _client.SendAsync(AuthorizedGet("/api/users?page=1&pageSize=2", adminToken));

        var result = await response.Content.ReadFromJsonAsync<PagedUsersResponse>(JsonOptions);
        Assert.Equal(2, result!.Items.Count);
        Assert.True(result.TotalCount >= 6);
    }
}
