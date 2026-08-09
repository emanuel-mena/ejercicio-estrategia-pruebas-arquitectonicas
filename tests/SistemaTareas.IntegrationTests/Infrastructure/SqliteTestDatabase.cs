using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaTareas.Infrastructure.Persistence;
using SistemaTareas.Infrastructure.Seed;

namespace SistemaTareas.IntegrationTests.Infrastructure;

internal sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        await using var context = CreateContext();
        await DatabaseSeeder.InitializeAsync(context);
    }

    public TareasDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TareasDbContext>()
            .UseSqlite(_connection)
            .Options;
        return new TareasDbContext(options);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}

