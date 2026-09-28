using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.IntegrationTests.Infrastructure.Repositories;

[TestFixture]
[Category("SqlIntegration")]
public class RoomRepositoryTests
{
    private SqlServerFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = new SqlServerFixture();

        await _fixture.InitializeAsync();

        await using var dbContext = _fixture.CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _fixture.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        await using var dbContext = _fixture.CreateDbContext();
        await dbContext.Rooms.ExecuteDeleteAsync();
    }

    [Test]
    public async Task AddAsync_SavesRoom()
    {
        // Arrange
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new RoomRepository(dbContext);

        var room = CreateRoom();

        // Act
        await repository.AddAsync(room);

        // Assert
        await using var assertContext = _fixture.CreateDbContext();

        var savedRoom = await assertContext.Rooms.SingleOrDefaultAsync(x => x.Id == room.Id);

        Assert.That(savedRoom, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(savedRoom!.Name, Is.EqualTo(room.Name));
            Assert.That(savedRoom.Capacity, Is.EqualTo(room.Capacity));
            Assert.That(savedRoom.HourlyRate, Is.EqualTo(room.HourlyRate));
        });
    }

    [Test]
    public async Task AddAsync_WithAvailableServices_PersistsRoomAndServices()
    {
        // Arrange
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room A",
            Capacity = 10,
            HourlyRate = 100m,
            AvailableServices =
            [
                new AdditionalService
                {
                    Id = Guid.NewGuid(),
                    Name = "Projector",
                    Price = 500m
                },
                new AdditionalService
                {
                    Id = Guid.NewGuid(),
                    Name = "Wi-Fi",
                    Price = 300m
                }
            ]
        };

        await using var dbContext = _fixture.CreateDbContext();
        var repository = new RoomRepository(dbContext);

        // Act
        await repository.AddAsync(room);

        // Assert
        await using var verificationContext = _fixture.CreateDbContext();

        var persistedRoom = await verificationContext.Rooms
            .Include(x => x.AvailableServices)
            .SingleAsync(x => x.Id == room.Id);

        Assert.Multiple(() =>
        {
            Assert.That(persistedRoom.AvailableServices, Has.Count.EqualTo(2));

            Assert.That(
                persistedRoom.AvailableServices.Any(x =>
                    x.Name == "Projector" &&
                    x.Price == 500m &&
                    x.RoomId == room.Id),
                Is.True);

            Assert.That(
                persistedRoom.AvailableServices.Any(x =>
                    x.Name == "Wi-Fi" &&
                    x.Price == 300m &&
                    x.RoomId == room.Id),
                Is.True);
        });
    }

    [Test]
    public async Task GetByIdAsync_WhenRoomExists_ReturnsRoom()
    {
        // Arrange
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new RoomRepository(dbContext);

        var room = CreateRoom();
        await repository.AddAsync(room);

        // Act
        var result = await repository.GetByIdAsync(room.Id);

        // Assert
        Assert.That(result, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(result!.Id, Is.EqualTo(room.Id));
            Assert.That(result.Name, Is.EqualTo(room.Name));
            Assert.That(result.Capacity, Is.EqualTo(room.Capacity));
            Assert.That(result.HourlyRate, Is.EqualTo(room.HourlyRate));
        });
    }

    [Test]
    public async Task GetByIdAsync_WhenRoomHasAvailableServices_ReturnsServices()
    {
        // Arrange
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room A",
            Capacity = 10,
            HourlyRate = 100m,
            AvailableServices =
            [
                new AdditionalService
            {
                Id = Guid.NewGuid(),
                Name = "Projector",
                Price = 500m
            },
            new AdditionalService
            {
                Id = Guid.NewGuid(),
                Name = "Wi-Fi",
                Price = 300m
            }
            ]
        };

        await using (var dbContext = _fixture.CreateDbContext())
        {
            var repository = new RoomRepository(dbContext);
            await repository.AddAsync(room);
        }

        // Act
        await using var queryContext = _fixture.CreateDbContext();
        var queryRepository = new RoomRepository(queryContext);

        var result = await queryRepository.GetByIdAsync(room.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.AvailableServices, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(
                result.AvailableServices.Any(x => x.Name == "Projector"),
                Is.True);

            Assert.That(
                result.AvailableServices.Any(x => x.Name == "Wi-Fi"),
                Is.True);
        });
    }

    [Test]
    public async Task GetByIdAsync_WhenRoomDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new RoomRepository(dbContext);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ReturnsRooms()
    {
        // Arrange
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new RoomRepository(dbContext);

        var room1 = CreateRoom("Room A");
        var room2 = CreateRoom("Room B");

        await repository.AddAsync(room1);
        await repository.AddAsync(room2);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result, Is.EquivalentTo(new[] { room1, room2 }).UsingPropertiesComparer());
        });
    }

    [Test]
    public async Task GetAllAsync_WhenRoomsHaveAvailableServices_ReturnsServices()
    {
        // Arrange
        var room1 = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room A",
            Capacity = 10,
            HourlyRate = 100m,
            AvailableServices =
            [
                new AdditionalService
                {
                    Id = Guid.NewGuid(),
                    Name = "Projector",
                    Price = 500m
                },
                new AdditionalService
                {
                    Id = Guid.NewGuid(),
                    Name = "Wi-Fi",
                    Price = 300m
                }
            ]
        };

        var room2 = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room B",
            Capacity = 20,
            HourlyRate = 150m,
            AvailableServices =
            [
                new AdditionalService
                {
                    Id = Guid.NewGuid(),
                    Name = "Sound",
                    Price = 700m
                }
            ]
        };

        await using (var dbContext = _fixture.CreateDbContext())
        {
            var repository = new RoomRepository(dbContext);

            await repository.AddAsync(room1);
            await repository.AddAsync(room2);
        }

        // Act
        await using var queryContext = _fixture.CreateDbContext();
        var queryRepository = new RoomRepository(queryContext);

        var result = await queryRepository.GetAllAsync();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));

        var returnedRoom1 = result.Single(x => x.Id == room1.Id);
        var returnedRoom2 = result.Single(x => x.Id == room2.Id);

        Assert.Multiple(() =>
        {
            Assert.That(returnedRoom1.AvailableServices, Has.Count.EqualTo(2));
            Assert.That(
                returnedRoom1.AvailableServices.Any(x => x.Name == "Projector"),
                Is.True);
            Assert.That(
                returnedRoom1.AvailableServices.Any(x => x.Name == "Wi-Fi"),
                Is.True);

            Assert.That(returnedRoom2.AvailableServices, Has.Count.EqualTo(1));
            Assert.That(
                returnedRoom2.AvailableServices.Single().Name,
                Is.EqualTo("Sound"));
        });
    }

    [Test]
    public async Task UpdateAsync_UpdatesRoom()
    {
        // Arrange
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new RoomRepository(dbContext);

        var room = CreateRoom();
        await repository.AddAsync(room);

        room.Name = "Updated Room";
        room.Capacity = 20;
        room.HourlyRate = 150m;

        // Act
        await repository.UpdateAsync(room);

        // Assert
        await using var assertContext = _fixture.CreateDbContext();

        var updatedRoom = await assertContext.Rooms.SingleAsync(x => x.Id == room.Id);

        Assert.Multiple(() =>
        {
            Assert.That(updatedRoom.Name, Is.EqualTo("Updated Room"));
            Assert.That(updatedRoom.Capacity, Is.EqualTo(20));
            Assert.That(updatedRoom.HourlyRate, Is.EqualTo(150m));
        });
    }

    [Test]
    public async Task DeleteAsync_RemovesRoom()
    {
        // Arrange
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new RoomRepository(dbContext);

        var room = CreateRoom();
        await repository.AddAsync(room);

        // Act
        await repository.DeleteAsync(room);

        // Assert
        await using var assertContext = _fixture.CreateDbContext();

        var exists = await assertContext.Rooms.AnyAsync(x => x.Id == room.Id);

        Assert.That(exists, Is.False);
    }

    private static Room CreateRoom(string? name = null)
    {
        return new Room
        {
            Id = Guid.NewGuid(),
            Name = name ?? $"Room-{Guid.NewGuid()}",
            Capacity = 10,
            HourlyRate = 100m
        };
    }
}
