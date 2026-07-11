using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Contracts.Auth;
using Api.Contracts.Users;

namespace Api.Tests;

/// <summary>
/// POST /api/users exige rol Admin desde la Iteración 11 — este helper
/// centraliza el flujo "login como el Admin sembrado -> crear -> login como
/// el usuario creado" que, si no, se repetiría en cada archivo de test.
/// </summary>
public static class TestUserFactory
{
    public const string SeededAdminEmail = "admin@workshop-cidenet.local";
    public const string SeededAdminPassword = "Admin123$";
    public const string DefaultPassword = "Clave123$";

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task<string> LoginAsSeededAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { Email = SeededAdminEmail, Password = SeededAdminPassword });
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        return body!.Token;
    }

    public static async Task<UserResponse> CreateAsync(HttpClient client, string email, string nombre, string rol, string? adminToken = null)
    {
        adminToken ??= await LoginAsSeededAdminAsync(client);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/users")
        {
            Content = JsonContent.Create(new
            {
                Nombre = nombre,
                Email = email,
                Password = DefaultPassword,
                ConfirmPassword = DefaultPassword,
                Rol = rol,
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions))!;
    }

    public static async Task<(Guid Id, string Token)> CreateAndLoginAsync(HttpClient client, string email, string nombre, string rol, string? adminToken = null)
    {
        var created = await CreateAsync(client, email, nombre, rol, adminToken);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = DefaultPassword });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);

        return (created.Id, login!.Token);
    }
}
