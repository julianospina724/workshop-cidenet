using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Contracts.Users;
using Application.Users;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.Tests;

public class CreateUserEndpointTests : IClassFixture<SqliteWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SqliteWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateUserEndpointTests(SqliteWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static object ValidPayload(string email) => new
    {
        Nombre = "Ana Pérez",
        Email = email,
        Password = "Clave123$",
        ConfirmPassword = "Clave123$",
        Rol = "Editor",
    };

    [Fact]
    public async Task Crear_cuenta_con_datos_validos_devuelve_201_y_queda_activa()
    {
        var response = await _client.PostAsJsonAsync("/api/users", ValidPayload("crear-valido@mail.com"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("crear-valido@mail.com", body!.Email);
        Assert.Equal(UserStatus.Activo, body.Estado);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = db.Users.Single(u => u.Email == "crear-valido@mail.com");
        Assert.Equal(UserStatus.Activo, stored.Estado);
    }

    [Fact]
    public async Task La_contrasena_y_confirmacion_no_coinciden_devuelve_400()
    {
        var payload = new
        {
            Nombre = "Ana Pérez",
            Email = "no-coincide@mail.com",
            Password = "Clave123$",
            ConfirmPassword = "Clave999$",
            Rol = "Editor",
        };

        var response = await _client.PostAsJsonAsync("/api/users", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.True(error!.FieldErrors is not null && error.FieldErrors.ContainsKey("confirmPassword"));
    }

    [Theory]
    [InlineData("nombre", "")]
    [InlineData("email", "correo-invalido")]
    [InlineData("password", "corta1")]
    [InlineData("password", "sololetrasminuscul")]
    public async Task Validar_reglas_de_formato_de_campos_obligatorios(string campo, string valor)
    {
        var payload = campo switch
        {
            "nombre" => new { Nombre = valor, Email = "formato-nombre@mail.com", Password = "Clave123$", ConfirmPassword = "Clave123$", Rol = "Editor" },
            "email" => new { Nombre = "Ana Pérez", Email = valor, Password = "Clave123$", ConfirmPassword = "Clave123$", Rol = "Editor" },
            _ => new { Nombre = "Ana Pérez", Email = $"formato-{valor}@mail.com", Password = valor, ConfirmPassword = valor, Rol = "Editor" },
        };

        var response = await _client.PostAsJsonAsync("/api/users", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.NotNull(error!.FieldErrors);
        Assert.Contains(campo, error.FieldErrors!.Keys);
    }

    [Fact]
    public async Task Email_ya_registrado_devuelve_400_con_mensaje_general()
    {
        var email = "duplicado@mail.com";
        await _client.PostAsJsonAsync("/api/users", ValidPayload(email));

        var response = await _client.PostAsJsonAsync("/api/users", ValidPayload(email));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Null(error!.FieldErrors);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
    }

    [Fact]
    public async Task Fallo_general_del_backend_devuelve_500()
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

        var response = await brokenFactory.CreateClient().PostAsJsonAsync("/api/users", ValidPayload("fallo-general@mail.com"));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Theory]
    [InlineData("  Ana   Pérez  ", "Normaliza@Mail.com", "Ana Pérez", "normaliza@mail.com")]
    public async Task Normaliza_nombre_y_email_antes_de_persistir(
        string nombreIngresado, string emailIngresado, string nombreEsperado, string emailEsperado)
    {
        var payload = new
        {
            Nombre = nombreIngresado,
            Email = emailIngresado,
            Password = "Clave123$",
            ConfirmPassword = "Clave123$",
            Rol = "Editor",
        };

        var response = await _client.PostAsJsonAsync("/api/users", payload);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = db.Users.Single(u => u.Email == emailEsperado);
        Assert.Equal(nombreEsperado, stored.Nombre);
    }

    [Fact]
    public async Task La_contrasena_nunca_se_retorna_en_la_respuesta_de_creacion()
    {
        var response = await _client.PostAsJsonAsync("/api/users", ValidPayload("sin-password-en-respuesta@mail.com"));

        var raw = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(raw);

        foreach (var property in json.RootElement.EnumerateObject())
        {
            Assert.DoesNotContain("password", property.Name, StringComparison.OrdinalIgnoreCase);
        }
    }

    private class ThrowingUserRepository : IUserRepository
    {
        public Task<bool> EmailExistsAsync(string email) => Task.FromResult(false);

        public Task<User?> FindByNormalizedEmailAsync(string normalizedEmail) => Task.FromResult<User?>(null);

        public Task<User?> FindByIdAsync(Guid id) => Task.FromResult<User?>(null);

        public Task<int> CountActiveAdminsAsync() => Task.FromResult(0);

        public Task<(IReadOnlyList<User> Items, int TotalCount)> SearchAsync(GetUsersQuery query) =>
            throw new InvalidOperationException("Fallo simulado para el test.");

        public Task AddAsync(User user) => throw new InvalidOperationException("Fallo simulado para el test.");

        public Task SaveChangesAsync() => throw new InvalidOperationException("Fallo simulado para el test.");
    }
}
