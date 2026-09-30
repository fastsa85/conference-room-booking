using Microsoft.Data.SqlClient;
using Respawn;

namespace ConferenceRoomBooking.TestInfrastructure;

public class DatabaseCleaner
{
    private readonly string _connectionString;
    private Respawner? _respawner;

    public DatabaseCleaner(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task CleanAsync()
    {
        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        _respawner ??= await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                TablesToIgnore =
                [
                    "__EFMigrationsHistory"
                ]
            });

        await _respawner.ResetAsync(connection);
    }
}
