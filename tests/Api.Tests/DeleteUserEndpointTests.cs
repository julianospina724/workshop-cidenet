using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Contracts.Auth;
using Api.Contracts.Users;
using Application.Users;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.Tests;

public class DeleteUserEndpointTests : IClassFixture<SqliteWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SqliteWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeleteUserEndpointTests(SqliteWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private Task<(Guid Id, string Token)> CreateUserAndLoginAsync(string email, string nombre, string rol) =>
        TestUserFactory.CreateAndLoginAsync(_client, email, nombre, rol);

    private HttpRequestMessage AuthorizedDelete(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    [Fact]
    public async Task Admin_elimina_un_usuario_queda_en_estado_eliminado()
    {
        var (_, adminToken) = await CreateUserAndLoginAsync("eliminar-admin@mail.com", "Admin Eliminar", "Admin");
        var (targetId, _) = await CreateUserAndLoginAsync("eliminar-objetivo@mail.com", "Objetivo Eliminar", "Editor");

        var response = await _client.SendAsync(AuthorizedDelete($"/api/users/{targetId}", adminToken));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = db.Users.Single(u => u.Id == targetId);
        Assert.Equal(UserStatus.Eliminado, stored.Estado);
    }

    [Fact]
    public async Task No_se_puede_eliminar_al_unico_admin_activo()
    {
        // Factory aislada (BD propia): el Admin sembrado por defecto también cuenta
        // como Admin activo, así que necesitamos neutralizarlo también — algo que
        // rompería a otros tests de esta clase si compartieran la misma BD.
        await using var isolatedFactory = new SqliteWebApplicationFactory();
        var isolatedClient = isolatedFactory.CreateClient();

        var (adminId, adminToken) = await TestUserFactory.CreateAndLoginAsync(isolatedClient, "unicoadmin-del@mail.com", "Unico Admin Delete", "Admin");

        using (var scope = isolatedFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var otrosAdmins = db.Users.Where(u => u.Rol == Role.Admin && u.Id != adminId && u.Estado == UserStatus.Activo);
            foreach (var otro in otrosAdmins)
            {
                otro.Estado = UserStatus.Inactivo;
            }

            await db.SaveChangesAsync();
        }

        var response = await isolatedClient.SendAsync(AuthorizedDelete($"/api/users/{adminId}", adminToken));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Solo_admin_puede_eliminar_autorizacion_server_side()
    {
        var (_, editorToken) = await CreateUserAndLoginAsync("soloadmin-del-editor@mail.com", "Editor Delete", "Editor");
        var (targetId, _) = await CreateUserAndLoginAsync("soloadmin-del-objetivo@mail.com", "Objetivo Delete", "Viewer");

        var response = await _client.SendAsync(AuthorizedDelete($"/api/users/{targetId}", editorToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Eliminar_un_usuario_ya_eliminado_devuelve_404()
    {
        var (_, adminToken) = await CreateUserAndLoginAsync("dobleeliminar-admin@mail.com", "Admin Doble Eliminar", "Admin");
        var (targetId, _) = await CreateUserAndLoginAsync("dobleeliminar-objetivo@mail.com", "Objetivo Doble Eliminar", "Editor");

        var primera = await _client.SendAsync(AuthorizedDelete($"/api/users/{targetId}", adminToken));
        Assert.Equal(HttpStatusCode.NoContent, primera.StatusCode);

        var segunda = await _client.SendAsync(AuthorizedDelete($"/api/users/{targetId}", adminToken));
        Assert.Equal(HttpStatusCode.NotFound, segunda.StatusCode);
    }

    [Fact]
    public async Task Eliminar_un_usuario_que_no_existe_devuelve_404()
    {
        var (_, adminToken) = await CreateUserAndLoginAsync("noexiste-del-admin@mail.com", "Admin No Existe Delete", "Admin");

        var response = await _client.SendAsync(AuthorizedDelete($"/api/users/{Guid.NewGuid()}", adminToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Usuario_eliminado_no_puede_autenticarse()
    {
        var (_, adminToken) = await CreateUserAndLoginAsync("elimauth-admin@mail.com", "Admin Elim Auth", "Admin");
        var email = "elimauth-objetivo@mail.com";
        var (targetId, _) = await CreateUserAndLoginAsync(email, "Objetivo Elim Auth", "Editor");

        await _client.SendAsync(AuthorizedDelete($"/api/users/{targetId}", adminToken));

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Clave123$" });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Fallo_general_del_backend_al_eliminar_devuelve_500()
    {
        var (targetId, adminToken) = await CreateUserAndLoginAsync("fallodel-admin@mail.com", "Admin Fallo Delete", "Admin");

        await using var brokenFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserRepository));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddScoped<IUserRepository>(_ => new ThrowingUserRepositoryForDelete());
            });
        });

        var response = await brokenFactory.CreateClient()
            .SendAsync(AuthorizedDelete($"/api/users/{targetId}", adminToken));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private class ThrowingUserRepositoryForDelete : IUserRepository
    {
        public Task<bool> EmailExistsAsync(string normalizedEmail) => Task.FromResult(false);

        public Task<User?> FindByNormalizedEmailAsync(string normalizedEmail) => Task.FromResult<User?>(null);

        public Task<User?> FindByIdAsync(Guid id) => throw new InvalidOperationException("Fallo simulado para el test.");

        public Task<int> CountActiveAdminsAsync() => throw new InvalidOperationException("Fallo simulado para el test.");

        public Task<(IReadOnlyList<User> Items, int TotalCount)> SearchAsync(GetUsersQuery query) =>
            throw new InvalidOperationException("Fallo simulado para el test.");

        public Task AddAsync(User user) => throw new InvalidOperationException("Fallo simulado para el test.");

        public Task SaveChangesAsync() => throw new InvalidOperationException("Fallo simulado para el test.");
    }
}
