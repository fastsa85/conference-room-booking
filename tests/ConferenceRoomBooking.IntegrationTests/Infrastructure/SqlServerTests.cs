namespace ConferenceRoomBooking.IntegrationTests.Infrastructure;

public class SqlServerTests
{
    [Test]
    public async Task SqlServerContainer_StartsAndAcceptsConnection()
    {
        // Arrange
        await using var fixture = new SqlServerFixture();

        // Act
        await fixture.InitializeAsync();

        await using var dbContext = fixture.CreateDbContext();

        var canConnect = await dbContext.Database.CanConnectAsync();

        // Assert
        Assert.That(canConnect, Is.True);
    }

    [Test]
    public async Task SqlServerContainer_CoreCanCreateDatabase()
    {
        // Arrange
        await using var fixture = new SqlServerFixture();

        // Act
        await fixture.InitializeAsync();

        await using var dbContext = fixture.CreateDbContext();

        await dbContext.Database.EnsureCreatedAsync();

        // Assert
        var canConnect = await dbContext.Database.CanConnectAsync();

        Assert.That(canConnect, Is.True);
    }
}
