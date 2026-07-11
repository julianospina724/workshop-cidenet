using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Api.Contracts.Auth;
using Api.Contracts.Users;
using Application.Common;
using Application.Users;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<CreateUserService>();
builder.Services.AddScoped<AuthenticateService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = JwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

const string FrontendCorsPolicy = "frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(
                builder.Configuration["FRONTEND_URL"] ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// En "Testing", SqliteWebApplicationFactory registra su propio AppDbContext
// (SQLite en memoria) — evitamos registrar Npgsql aquí para no tener dos
// proveedores de EF Core compitiendo en el mismo contenedor de servicios.
if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
        ?? "Host=localhost;Port=5432;Database=workshop_cidenet;Username=postgres;Password=postgres";

    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new ErrorResponse("Ocurrió un error inesperado. Intenta de nuevo.", null));
}));

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("HealthCheck");

app.MapPost("/api/users", async (CreateUserCommand command, CreateUserService service) =>
{
    var result = await service.CreateAsync(command);

    if (!result.Succeeded)
    {
        return Results.BadRequest(new ErrorResponse(result.Message, result.FieldErrors));
    }

    var response = UserResponse.FromEntity(result.User!);
    return Results.Created($"/api/users/{response.Id}", response);
}).WithName("CreateUser");

app.MapPost("/api/auth/login", async (AuthenticateCommand command, AuthenticateService service) =>
{
    var result = await service.AuthenticateAsync(command);

    if (result.LockedOut)
    {
        return Results.Json(
            new ErrorResponse("La cuenta está bloqueada temporalmente por intentos fallidos. Intenta de nuevo más tarde.", null),
            statusCode: StatusCodes.Status423Locked);
    }

    if (!result.Succeeded)
    {
        return Results.Json(
            new ErrorResponse("Email o contraseña incorrectos.", null),
            statusCode: StatusCodes.Status401Unauthorized);
    }

    return Results.Ok(new LoginResponse(result.Token!, UserResponse.FromEntity(result.User!)));
}).WithName("Login");

app.MapGet("/api/auth/whoami", (ClaimsPrincipal user) =>
{
    var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    var rol = user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    return Results.Ok(new WhoAmIResponse(id, rol));
}).RequireAuthorization().WithName("WhoAmI");

// El AppDbContext queda registrado y listo. Cuando definas tu dominio y tu
// primera migración, aplícala al arrancar (ej.):
//   if (!app.Environment.IsEnvironment("Testing"))
//   {
//       using var scope = app.Services.CreateScope();
//       await scope.ServiceProvider.GetRequiredService<AppDbContext>()
//           .Database.MigrateAsync();
//   }

app.Run();

public partial class Program
{
}
