using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Contracts.Auth;
using Api.Contracts.Users;
using Application.Users;
using Domain.Users;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.Tests;

public class EditUserEndpointTests : IClassFixture<SqliteWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SqliteWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EditUserEndpointTests(SqliteWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid Id, string Token)> CreateUserAndLoginAsync(string email, string nombre, string rol)
    {
        var createResponse = await _client.PostAsJsonAsync("/api/users", new
        {
            Nombre = nombre,
            Email = email,
            Password = "Clave123$",
            ConfirmPassword = "Clave123$",
            Rol = rol,
        });
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Clave123$" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);

        return (created!.Id, login!.Token);
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
    public async Task Admin_edita_los_datos_de_otro_usuario()
    {
        var (adminId, adminToken) = await CreateUserAndLoginAsync("editar-admin@mail.com", "Admin Editor Uno", "Admin");
        var (targetId, _) = await CreateUserAndLoginAsync("editar-objetivo@mail.com", "Objetivo Original", "Editor");

        var response = await _client.SendAsync(AuthorizedPut($"/api/users/{targetId}", adminToken, new { Rol = "Viewer" }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);
        Assert.Equal(Role.Viewer, body!.Rol);
        Assert.NotEqual(adminId, targetId);
    }

    [Fact]
    public async Task Intentar_cambiar_el_propio_rol_es_rechazado()
    {
        var (adminId, adminToken) = await CreateUserAndLoginAsync("propiorol-admin@mail.com", "Admin Propio Rol", "Admin");
        await CreateUserAndLoginAsync("propiorol-otro-admin@mail.com", "Otro Admin", "Admin");

        var response = await _client.SendAsync(AuthorizedPut($"/api/users/{adminId}", adminToken, new { Rol = "Editor" }));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Email_duplicado_al_editar_devuelve_400()
    {
        var (_, adminToken) = await CreateUserAndLoginAsync("emailedit-admin@mail.com", "Admin Email Edit", "Admin");
        await CreateUserAndLoginAsync("emailedit-existente@mail.com", "Usuario Existente", "Editor");
        var (targetId, _) = await CreateUserAndLoginAsync("emailedit-objetivo@mail.com", "Usuario Objetivo", "Editor");

        var response = await _client.SendAsync(AuthorizedPut($"/api/users/{targetId}", adminToken, new { Email = "emailedit-existente@mail.com" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.False(string.IsNullOrWhiteSpace(error!.Message));
    }

    [Fact]
    public async Task El_unico_admin_activo_no_puede_autodesactivarse()
    {
        var (adminId, adminToken) = await CreateUserAndLoginAsync("unicoadmin@mail.com", "Unico Admin", "Admin");

        // La fixture comparte base de datos entre los tests de esta clase, así que
        // neutralizamos cualquier otro Admin creado por otros tests para que este
        // sea, de verdad, el único Admin activo al momento de la aserción.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.AppDbContext>();
            var otrosAdmins = db.Users.Where(u => u.Rol == Role.Admin && u.Id != adminId && u.Estado == UserStatus.Activo);
            foreach (var otro in otrosAdmins)
            {
                otro.Estado = UserStatus.Inactivo;
            }

            await db.SaveChangesAsync();
        }

        var response = await _client.SendAsync(AuthorizedPut($"/api/users/{adminId}", adminToken, new { Estado = "Inactivo" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Solo_admin_puede_editar_autorizacion_server_side()
    {
        var (_, editorToken) = await CreateUserAndLoginAsync("soloadmin-editor@mail.com", "Editor Autorizacion", "Editor");
        var (targetId, _) = await CreateUserAndLoginAsync("soloadmin-objetivo@mail.com", "Objetivo Autorizacion", "Viewer");

        var response = await _client.SendAsync(AuthorizedPut($"/api/users/{targetId}", editorToken, new { Nombre = "Intento De Cambio" }));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task La_respuesta_de_editar_nunca_expone_la_contrasena()
    {
        var (_, adminToken) = await CreateUserAndLoginAsync("editresp-admin@mail.com", "Admin Editar Respuesta", "Admin");
        var (targetId, _) = await CreateUserAndLoginAsync("editresp-objetivo@mail.com", "Objetivo Respuesta", "Editor");

        var response = await _client.SendAsync(AuthorizedPut($"/api/users/{targetId}", adminToken, new { Nombre = "Nombre Actualizado" }));
        var raw = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("hash", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Normaliza_el_nombre_antes_de_persistir()
    {
        var (_, adminToken) = await CreateUserAndLoginAsync("normaliza-admin@mail.com", "Admin Normaliza", "Admin");
        var (targetId, _) = await CreateUserAndLoginAsync("normaliza-objetivo@mail.com", "Objetivo Normaliza", "Editor");

        var response = await _client.SendAsync(AuthorizedPut($"/api/users/{targetId}", adminToken, new { Nombre = "  Nombre   Con Espacios  " }));

        var body = await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);
        Assert.Equal("Nombre Con Espacios", body!.Nombre);
    }

    [Fact]
    public async Task Editar_un_usuario_que_no_existe_devuelve_404()
    {
        var (_, adminToken) = await CreateUserAndLoginAsync("noexiste-admin@mail.com", "Admin No Existe", "Admin");

        var response = await _client.SendAsync(AuthorizedPut($"/api/users/{Guid.NewGuid()}", adminToken, new { Nombre = "Cualquiera" }));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Fallo_general_del_backend_al_editar_devuelve_500()
    {
        var (targetId, adminToken) = await CreateUserAndLoginAsync("fallogeneral-admin@mail.com", "Admin Fallo General", "Admin");

        await using var brokenFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserRepository));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddScoped<IUserRepository>(_ => new ThrowingUserRepositoryForEdit(targetId));
            });
        });

        var response = await brokenFactory.CreateClient()
            .SendAsync(AuthorizedPut($"/api/users/{targetId}", adminToken, new { Nombre = "Cualquiera" }));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private class ThrowingUserRepositoryForEdit : IUserRepository
    {
        private readonly Guid _existingUserId;

        public ThrowingUserRepositoryForEdit(Guid existingUserId)
        {
            _existingUserId = existingUserId;
        }

        public Task<bool> EmailExistsAsync(string normalizedEmail) => Task.FromResult(false);

        public Task<User?> FindByNormalizedEmailAsync(string normalizedEmail) => Task.FromResult<User?>(null);

        public Task<(IReadOnlyList<User> Items, int TotalCount)> SearchAsync(GetUsersQuery query) =>
            throw new InvalidOperationException("Fallo simulado para el test.");

        public Task<User?> FindByIdAsync(Guid id) => throw new InvalidOperationException("Fallo simulado para el test.");

        public Task<int> CountActiveAdminsAsync() => throw new InvalidOperationException("Fallo simulado para el test.");

        public Task AddAsync(User user) => throw new InvalidOperationException("Fallo simulado para el test.");

        public Task SaveChangesAsync() => throw new InvalidOperationException("Fallo simulado para el test.");
    }
}
