using System.Net;
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

public class AuthenticateEndpointTests : IClassFixture<SqliteWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SqliteWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthenticateEndpointTests(SqliteWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task SeedUserAsync(string email, string password = "Clave123$", UserStatus estado = UserStatus.Activo)
    {
        var createResponse = await _client.PostAsJsonAsync("/api/users", new
        {
            Nombre = "Usuario Prueba",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            Rol = "Editor",
        });
        createResponse.EnsureSuccessStatusCode();

        if (estado != UserStatus.Activo)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = db.Users.Single(u => u.Email == email.ToLowerInvariant());
            user.Estado = estado;
            await db.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Autenticarse_con_credenciales_correctas_devuelve_200()
    {
        await SeedUserAsync("login-valido@mail.com");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "login-valido@mail.com", Password = "Clave123$" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal("login-valido@mail.com", body.User.Email);
    }

    [Fact]
    public async Task Credenciales_incorrectas_devuelve_401_con_mensaje_generico()
    {
        await SeedUserAsync("credenciales-malas@mail.com");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "credenciales-malas@mail.com", Password = "OtraClave1$" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("Email o contraseña incorrectos.", error!.Message);
    }

    [Fact]
    public async Task Usuario_inactivo_con_credenciales_correctas_devuelve_401_mismo_mensaje()
    {
        await SeedUserAsync("inactivo@mail.com", estado: UserStatus.Inactivo);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "inactivo@mail.com", Password = "Clave123$" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("Email o contraseña incorrectos.", error!.Message);
    }

    [Fact]
    public async Task Usuario_eliminado_no_puede_autenticarse()
    {
        await SeedUserAsync("eliminado@mail.com", estado: UserStatus.Eliminado);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "eliminado@mail.com", Password = "Clave123$" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("Normalizado1@Mail.com", "normalizado1@mail.com")]
    [InlineData("NORMALIZADO2@MAIL.COM", "normalizado2@mail.com")]
    public async Task Login_normaliza_el_email_a_minusculas(string emailIngresado, string emailSemilla)
    {
        await SeedUserAsync(emailSemilla);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = emailIngresado, Password = "Clave123$" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Bloqueo_tras_3_intentos_fallidos_consecutivos()
    {
        await SeedUserAsync("bloqueo@mail.com");

        for (var i = 0; i < 3; i++)
        {
            await _client.PostAsJsonAsync("/api/auth/login", new { Email = "bloqueo@mail.com", Password = "ClaveIncorrecta1$" });
        }

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "bloqueo@mail.com", Password = "Clave123$" });

        Assert.Equal((HttpStatusCode)423, response.StatusCode);
    }

    [Fact]
    public async Task Intentar_autenticarse_mientras_bloqueada_devuelve_423()
    {
        await SeedUserAsync("bloqueada-persistente@mail.com");

        for (var i = 0; i < 3; i++)
        {
            await _client.PostAsJsonAsync("/api/auth/login", new { Email = "bloqueada-persistente@mail.com", Password = "ClaveIncorrecta1$" });
        }

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "bloqueada-persistente@mail.com", Password = "ClaveIncorrecta1$" });

        Assert.Equal((HttpStatusCode)423, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Contains("bloqueada", error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task El_bloqueo_se_libera_automaticamente_despues_de_15_minutos()
    {
        await SeedUserAsync("libera-bloqueo@mail.com");
        _factory.Clock.UtcNow = DateTime.UtcNow;

        for (var i = 0; i < 3; i++)
        {
            await _client.PostAsJsonAsync("/api/auth/login", new { Email = "libera-bloqueo@mail.com", Password = "ClaveIncorrecta1$" });
        }

        var stillLocked = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "libera-bloqueo@mail.com", Password = "Clave123$" });
        Assert.Equal((HttpStatusCode)423, stillLocked.StatusCode);

        _factory.Clock.UtcNow = _factory.Clock.UtcNow.AddMinutes(15).AddSeconds(1);

        var afterWaiting = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "libera-bloqueo@mail.com", Password = "Clave123$" });
        Assert.Equal(HttpStatusCode.OK, afterWaiting.StatusCode);
    }

    [Fact]
    public async Task El_conteo_de_intentos_fallidos_se_reinicia_tras_login_exitoso()
    {
        await SeedUserAsync("reinicio-conteo@mail.com");

        await _client.PostAsJsonAsync("/api/auth/login", new { Email = "reinicio-conteo@mail.com", Password = "ClaveIncorrecta1$" });
        await _client.PostAsJsonAsync("/api/auth/login", new { Email = "reinicio-conteo@mail.com", Password = "ClaveIncorrecta1$" });

        var success = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "reinicio-conteo@mail.com", Password = "Clave123$" });
        Assert.Equal(HttpStatusCode.OK, success.StatusCode);

        var afterOneMoreFailure = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "reinicio-conteo@mail.com", Password = "ClaveIncorrecta1$" });

        Assert.Equal(HttpStatusCode.Unauthorized, afterOneMoreFailure.StatusCode);
    }

    [Fact]
    public async Task Fallo_general_del_backend_al_autenticarse_devuelve_500()
    {
        await using var brokenFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserRepository));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddScoped<IUserRepository, ThrowingUserRepository>();
            });
        });

        var response = await brokenFactory.CreateClient()
            .PostAsJsonAsync("/api/auth/login", new { Email = "cualquiera@mail.com", Password = "Clave123$" });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private class ThrowingUserRepository : IUserRepository
    {
        public Task<bool> EmailExistsAsync(string normalizedEmail) => Task.FromResult(false);

        public Task<User?> FindByNormalizedEmailAsync(string normalizedEmail) =>
            throw new InvalidOperationException("Fallo simulado para el test.");

        public Task<User?> FindByIdAsync(Guid id) => throw new InvalidOperationException("Fallo simulado para el test.");

        public Task<int> CountActiveAdminsAsync() => throw new InvalidOperationException("Fallo simulado para el test.");

        public Task<(IReadOnlyList<User> Items, int TotalCount)> SearchAsync(GetUsersQuery query) =>
            throw new InvalidOperationException("Fallo simulado para el test.");

        public Task AddAsync(User user) => throw new InvalidOperationException("Fallo simulado para el test.");

        public Task SaveChangesAsync() => throw new InvalidOperationException("Fallo simulado para el test.");
    }
}
