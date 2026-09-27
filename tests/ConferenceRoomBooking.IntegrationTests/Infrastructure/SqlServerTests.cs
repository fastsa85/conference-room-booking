namespace ConferenceRoomBooking.IntegrationTests.Infrastructure;

[TestFixture]
[Category("SqlIntegration")]
public class SqlServerTests
{
    private SqlServerFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = new SqlServerFixture();

        await _fixture.InitializeAsync();

        await using var dbContext = _fixture.CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _fixture.DisposeAsync();
    }

    [Test]
    public async Task SqlServerContainer_StartsAndAcceptsConnection()
    {
        // Arrange
        await using var dbContext = _fixture.CreateDbContext();

        // Act
        var canConnect = await dbContext.Database.CanConnectAsync();

        // Assert
        Assert.That(canConnect, Is.True);
    }

    [Test]
    public async Task SqlServerContainer_CoreCanCreateDatabase()
    {
        // Arrange
        await using var dbContext = _fixture.CreateDbContext();

        // Act
        await dbContext.Database.EnsureCreatedAsync();

        // Assert
        var canConnect = await dbContext.Database.CanConnectAsync();

        Assert.That(canConnect, Is.True);
    }
}
