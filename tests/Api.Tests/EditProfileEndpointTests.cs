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

public class EditProfileEndpointTests : IClassFixture<SqliteWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SqliteWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EditProfileEndpointTests(SqliteWebApplicationFactory factory)
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

    private HttpRequestMessage AuthorizedPutRaw(string token, string rawJsonBody)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/users/me")
        {
            Content = new StringContent(rawJsonBody, System.Text.Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private HttpRequestMessage AuthorizedPut(string token, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/users/me")
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    [Fact]
    public async Task Edito_mi_propio_nombre_y_email()
    {
        var (_, token) = await CreateUserAndLoginAsync("perfil-propio@mail.com", "Nombre Original", "Editor");

        var response = await _client.SendAsync(AuthorizedPut(token, new { Nombre = "Nombre Actualizado", Email = "perfil-propio-nuevo@mail.com" }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);
        Assert.Equal("Nombre Actualizado", body!.Nombre);
        Assert.Equal("perfil-propio-nuevo@mail.com", body.Email);
    }

    [Fact]
    public async Task Email_duplicado_al_editar_mi_perfil_devuelve_400()
    {
        await CreateUserAndLoginAsync("perfil-existente@mail.com", "Usuario Existente", "Editor");
        var (_, token) = await CreateUserAndLoginAsync("perfil-propio-dup@mail.com", "Usuario Propio", "Editor");

        var response = await _client.SendAsync(AuthorizedPut(token, new { Email = "perfil-existente@mail.com" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Enviar_rol_extra_por_manipulacion_directa_no_tiene_efecto()
    {
        var (_, token) = await CreateUserAndLoginAsync("perfil-rol-extra@mail.com", "Usuario Rol Extra", "Editor");

        var response = await _client.SendAsync(AuthorizedPutRaw(token, "{\"nombre\":\"Con Rol Extra\",\"rol\":\"Admin\",\"estado\":\"Inactivo\"}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);
        Assert.Equal(Role.Editor, body!.Rol);
        Assert.Equal(UserStatus.Activo, body.Estado);
    }

    [Fact]
    public async Task Mi_cuenta_inactiva_al_momento_de_guardar_es_rechazada()
    {
        var (userId, token) = await CreateUserAndLoginAsync("perfil-desactivado@mail.com", "Usuario Desactivado", "Editor");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = db.Users.Single(u => u.Id == userId);
            user.Estado = UserStatus.Inactivo;
            await db.SaveChangesAsync();
        }

        var response = await _client.SendAsync(AuthorizedPut(token, new { Nombre = "Intento De Guardar" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Normaliza_mi_nombre_antes_de_persistir()
    {
        var (_, token) = await CreateUserAndLoginAsync("perfil-normaliza@mail.com", "Usuario Normaliza", "Editor");

        var response = await _client.SendAsync(AuthorizedPut(token, new { Nombre = "  Nombre   Con Espacios  " }));

        var body = await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);
        Assert.Equal("Nombre Con Espacios", body!.Nombre);
    }

    [Fact]
    public async Task Fallo_general_al_guardar_mi_perfil_devuelve_500()
    {
        var (_, token) = await CreateUserAndLoginAsync("perfil-fallo-general@mail.com", "Usuario Fallo General", "Editor");

        await using var brokenFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserRepository));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddScoped<IUserRepository, ThrowingUserRepositoryForProfile>();
            });
        });

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/users/me")
        {
            Content = JsonContent.Create(new { Nombre = "Cualquiera" }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await brokenFactory.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private class ThrowingUserRepositoryForProfile : IUserRepository
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
