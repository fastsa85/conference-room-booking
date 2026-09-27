using System;
using ConferenceRoomBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace ConferenceRoomBooking.IntegrationTests.Infrastructure;

public class SqlServerFixture : IAsyncDisposable
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public string ConnectionString => _msSqlContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
    }       

    public ValueTask DisposeAsync()
    {
        return _msSqlContainer.DisposeAsync();
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }
}
