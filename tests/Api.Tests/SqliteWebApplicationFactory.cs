using Application.Common;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

/// <summary>
/// WebApplicationFactory con SQLite en memoria en vez de Postgres — rápida y
/// determinista para tests de integración HTTP, pero respeta restricciones
/// relacionales reales (a diferencia del proveedor InMemory de EF). También
/// reemplaza IClock por un FakeClock controlable (necesario para probar el
/// bloqueo temporal de 15 minutos sin esperar de verdad).
/// </summary>
public class SqliteWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public FakeClock Clock { get; } = new();

    public SqliteWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbOptionsDescriptor is not null)
            {
                services.Remove(dbOptionsDescriptor);
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            var clockDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IClock));
            if (clockDescriptor is not null)
            {
                services.Remove(clockDescriptor);
            }

            services.AddSingleton<IClock>(Clock);

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
