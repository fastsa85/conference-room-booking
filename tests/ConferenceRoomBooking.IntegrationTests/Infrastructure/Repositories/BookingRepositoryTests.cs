using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Domain.Enums;
using ConferenceRoomBooking.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.IntegrationTests.Infrastructure.Repositories;

[TestFixture]
[Category("SqlIntegration")]
public class BookingRepositoryTests
{
    private SqlServerFixture _fixture = null!;
    private DatabaseCleaner _databaseCleaner = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = new SqlServerFixture();

        await _fixture.InitializeAsync();

        await using var dbContext = _fixture.CreateDbContext();
        await dbContext.Database.MigrateAsync();

        _databaseCleaner = new DatabaseCleaner(_fixture.ConnectionString);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _fixture.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        await _databaseCleaner.CleanAsync();
    }

    [Test]
    public async Task GetRoomForBookingAsync_WhenRoomExists_ReturnsRoomWithServices()
    {
        // Arrange
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room A",
            Capacity = 50,
            HourlyRate = 2000m,
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
            var roomRepository = new RoomRepository(dbContext);
            await roomRepository.AddAsync(room);
        }

        // Act
        await using var queryContext = _fixture.CreateDbContext();
        var repository = new BookingRepository(queryContext);

        var result = await repository.GetRoomForBookingAsync(room.Id);

        // Assert
        Assert.That(result, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(result!.Id, Is.EqualTo(room.Id));
            Assert.That(result.Name, Is.EqualTo("Meeting Room A"));
            Assert.That(result.Capacity, Is.EqualTo(50));
            Assert.That(result.HourlyRate, Is.EqualTo(2000m));
            Assert.That(result.AvailableServices, Has.Count.EqualTo(2));

            Assert.That(result.AvailableServices.Any(x => x.Name == "Projector"), Is.True);

            Assert.That(result.AvailableServices.Any(x => x.Name == "Wi-Fi"), Is.True);
        });
    }

    [Test]
    public async Task GetRoomForBookingAsync_WhenRoomDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new BookingRepository(dbContext);

        // Act
        var result = await repository.GetRoomForBookingAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task HasOverlappingBookingAsync_WhenConfirmedBookingOverlaps_ReturnsTrue()
    {
        // Arrange
        var room = CreateRoom();

        await using (var dbContext = _fixture.CreateDbContext())
        {
            var roomRepository = new RoomRepository(dbContext);
            await roomRepository.AddAsync(room);

            dbContext.Bookings.Add(new Booking
            {
                Id = Guid.NewGuid(),
                RoomId = room.Id,
                Status = BookingStatus.Confirmed,
                Start = new DateTime(2026, 10, 1, 10, 0, 0),
                End = new DateTime(2026, 10, 1, 14, 0, 0),
                TotalCost = 4000m
            });

            await dbContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateDbContext();
        var repository = new BookingRepository(queryContext);

        // Act
        var result = await repository.HasOverlappingBookingAsync(
            room.Id,
            new DateTime(2026, 10, 1, 12, 0, 0),
            new DateTime(2026, 10, 1, 16, 0, 0));

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasOverlappingBookingAsync_WhenBookingTouchesBoundary_ReturnsFalse()
    {
        // Arrange
        var room = CreateRoom();

        await using (var dbContext = _fixture.CreateDbContext())
        {
            var roomRepository = new RoomRepository(dbContext);
            await roomRepository.AddAsync(room);

            dbContext.Bookings.Add(new Booking
            {
                Id = Guid.NewGuid(),
                RoomId = room.Id,
                Status = BookingStatus.Confirmed,
                Start = new DateTime(2026, 10, 1, 10, 0, 0),
                End = new DateTime(2026, 10, 1, 14, 0, 0),
                TotalCost = 4000m
            });

            await dbContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateDbContext();
        var repository = new BookingRepository(queryContext);

        // Act
        var result = await repository.HasOverlappingBookingAsync(
            room.Id,
            new DateTime(2026, 10, 1, 14, 0, 0),
            new DateTime(2026, 10, 1, 16, 0, 0));

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasOverlappingBookingAsync_WhenBookingIsCancelled_ReturnsFalse()
    {
        // Arrange
        var room = CreateRoom();

        await using (var dbContext = _fixture.CreateDbContext())
        {
            var roomRepository = new RoomRepository(dbContext);
            await roomRepository.AddAsync(room);

            dbContext.Bookings.Add(new Booking
            {
                Id = Guid.NewGuid(),
                RoomId = room.Id,
                Status = BookingStatus.Cancelled,
                Start = new DateTime(2026, 10, 1, 10, 0, 0),
                End = new DateTime(2026, 10, 1, 14, 0, 0),
                TotalCost = 4000m
            });

            await dbContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateDbContext();
        var repository = new BookingRepository(queryContext);

        // Act
        var result = await repository.HasOverlappingBookingAsync(
            room.Id,
            new DateTime(2026, 10, 1, 12, 0, 0),
            new DateTime(2026, 10, 1, 16, 0, 0));

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task AddAsync_PersistsBookingWithSelectedServices()
    {
        // Arrange
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room A",
            Capacity = 50,
            HourlyRate = 2000m
        };

        var projector = new AdditionalService
        {
            Id = Guid.NewGuid(),
            Name = "Projector",
            Price = 500m,
            RoomId = room.Id
        };

        room.AvailableServices.Add(projector);

        await using (var dbContext = _fixture.CreateDbContext())
        {
            var roomRepository = new RoomRepository(dbContext);
            await roomRepository.AddAsync(room);
        }

        var boolingId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = boolingId,
            RoomId = room.Id,
            Status = BookingStatus.Confirmed,
            Start = new DateTime(2026, 10, 1, 10, 0, 0),
            End = new DateTime(2026, 10, 1, 14, 0, 0),
            TotalCost = 8500m,
            AdditionalServices =
            [
                new BookingAdditionalService
                {
                    Id = Guid.NewGuid(),
                    BookingId = boolingId,
                    AdditionalServiceId = projector.Id,
                    Price = projector.Price
                }
            ]
        };

        await using (var dbContext = _fixture.CreateDbContext())
        {
            var repository = new BookingRepository(dbContext);

            // Act
            await repository.AddAsync(booking);
        }

        // Assert
        await using var assertContext = _fixture.CreateDbContext();

        var savedBooking = await assertContext.Bookings
            .Include(x => x.AdditionalServices)
            .SingleAsync(x => x.Id == booking.Id);

        Assert.Multiple(() =>
        {
            Assert.That(savedBooking.RoomId, Is.EqualTo(room.Id));
            Assert.That(savedBooking.Status, Is.EqualTo(BookingStatus.Confirmed));
            Assert.That(savedBooking.Start, Is.EqualTo(booking.Start));
            Assert.That(savedBooking.End, Is.EqualTo(booking.End));
            Assert.That(savedBooking.TotalCost, Is.EqualTo(8500m));

            Assert.That(
                savedBooking.AdditionalServices,
                Has.Count.EqualTo(1));

            Assert.That(
                savedBooking.AdditionalServices.Single().AdditionalServiceId,
                Is.EqualTo(projector.Id));

            Assert.That(
                savedBooking.AdditionalServices.Single().Price,
                Is.EqualTo(500m));
        });
    }

    private static Room CreateRoom()
    {
        return new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room A",
            Capacity = 50,
            HourlyRate = 2000m
        };
    }
}