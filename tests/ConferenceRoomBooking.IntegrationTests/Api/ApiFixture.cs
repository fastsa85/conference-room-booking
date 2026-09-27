using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Testcontainers.MsSql;

namespace ConferenceRoomBooking.IntegrationTests.Api;

public class ApiFixture : IAsyncDisposable
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(service =>service.ServiceType == typeof(DbContextOptions<AppDbContext>));

                    if (descriptor is not null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AppDbContext>(options => 
                    {
                        options.UseSqlServer(_msSqlContainer.GetConnectionString());
                    });
                });
            });

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();

        Client = _factory.CreateClient();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Rooms.ExecuteDeleteAsync();
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _msSqlContainer.DisposeAsync();
    }

    public async Task<Room?> GetRoomAsync(Guid id)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        return await dbContext.Rooms
            .AsNoTracking()
            .SingleOrDefaultAsync(room => room.Id == id);
    }

    public async Task AddRoomAsync(Room room)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync();
    }
}

