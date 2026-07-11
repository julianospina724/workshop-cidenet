using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Contracts.Auth;
using Infrastructure.Security;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Api.Tests;

public class JwtAuthenticationTests : IClassFixture<SqliteWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SqliteWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public JwtAuthenticationTests(SqliteWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> SeedUserAndLoginAsync(string email)
    {
        var (_, token) = await TestUserFactory.CreateAndLoginAsync(_client, email, "Usuario JWT", "Admin");
        return token;
    }

    [Fact]
    public async Task Login_exitoso_incluye_un_token_no_vacio()
    {
        var token = await SeedUserAndLoginAsync("jwt-login@mail.com");

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Token_valido_permite_acceder_a_whoami_y_expone_id_y_rol()
    {
        var token = await SeedUserAndLoginAsync("jwt-whoami@mail.com");

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/whoami");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<WhoAmIResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("Admin", body!.Rol);
        Assert.False(string.IsNullOrWhiteSpace(body.Id));
    }

    [Fact]
    public async Task Sin_token_el_endpoint_protegido_devuelve_401()
    {
        var response = await _client.GetAsync("/api/auth/whoami");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Token_manipulado_devuelve_401()
    {
        var token = await SeedUserAndLoginAsync("jwt-manipulado@mail.com");

        // Cambiamos un carácter en el medio del token (no el último): el último
        // caracter de la firma en Base64URL a veces cae en bits de relleno no
        // significativos, y no siempre invalida la firma al decodificarse.
        var middle = token.Length / 2;
        var tamperedToken = token[..middle] + (token[middle] == 'a' ? 'b' : 'a') + token[(middle + 1)..];

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/whoami");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tamperedToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Token_expirado_devuelve_401()
    {
        var expiredToken = CreateTokenExpiredMinutesAgo(5);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/whoami");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static string CreateTokenExpiredMinutesAgo(int minutesAgo)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Admin"),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: JwtSettings.Issuer,
            audience: JwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-minutesAgo - 1),
            expires: DateTime.UtcNow.AddMinutes(-minutesAgo),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
