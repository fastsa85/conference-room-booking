using ConferenceRoomBooking.Application.Rooms;
using ConferenceRoomBooking.Domain.Entities;
using Moq;

namespace ConferenceRoomBooking.UnitTests.Application.Rooms;

[TestFixture]
public class RoomServiceTests
{
    private Mock<IRoomRepository> _roomRepository = null!;
    private RoomService _roomService = null!;

    [SetUp]
    public void SetUp()
    {
        _roomRepository = new Mock<IRoomRepository>();
        _roomService = new RoomService(_roomRepository.Object);
    }

    [Test]
    public async Task GetAllAsync_ReturnsRooms()
    {
        // Arrange
        var rooms = new List<Room>
        {
            CreateRoom("Room A"),
            CreateRoom("Room B")
        };

        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(rooms);

        // Act
        var result = await _roomService.GetAllAsync();

        // Assert
        Assert.That(result, Is.SameAs(rooms));
    }

    [Test]
    public async Task GetAllAsync_WhenNoRoomsExist_ReturnsEmptyCollection()
    {
        // Arrange
        var rooms = new List<Room>();

        _roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(rooms);

        // Act
        var result = await _roomService.GetAllAsync();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetByIdAsync_WhenRoomExists_ReturnsRoom()
    {
        // Arrange
        var room = CreateRoom();

        _roomRepository.Setup(x => x.GetByIdAsync(room.Id)).ReturnsAsync(room);

        // Act
        var result = await _roomService.GetByIdAsync(room.Id);

        // Assert
        Assert.That(result, Is.SameAs(room));
    }

    [Test]
    public async Task GetByIdAsync_WhenRoomDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _roomRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Room?)null);

        // Act
        var result = await _roomService.GetByIdAsync(id);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_WithValidData_CreatesRoom()
    {
        // Arrange
        const string name = "Room A";
        const int capacity = 10;
        const decimal hourlyRate = 100m;
        var availableServices = new[]
        {
            new AdditionalServiceInput("Projector", 500m),
            new AdditionalServiceInput("Wi-Fi", 300m)
        };

        // Act
        var result = await _roomService.CreateAsync(name, capacity, hourlyRate, availableServices);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.Capacity, Is.EqualTo(capacity));
            Assert.That(result.HourlyRate, Is.EqualTo(hourlyRate));

            Assert.That(result.AvailableServices, Has.Count.EqualTo(2));

            Assert.That(
                result.AvailableServices.Any(service =>
                    service.Id != Guid.Empty &&
                    service.Name == "Projector" &&
                    service.Price == 500m),
                Is.True);
            Assert.That(
                result.AvailableServices.Any(service =>
                    service.Id != Guid.Empty &&
                    service.Name == "Wi-Fi" &&
                    service.Price == 300m),
                Is.True);
        });

        _roomRepository.Verify(
            x => x.AddAsync(It.Is<Room>(room =>
                room.Id == result.Id &&
                room.Name == name &&
                room.Capacity == capacity &&
                room.HourlyRate == hourlyRate &&
                room.AvailableServices.Count == 2)),
            Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenRoomExists_UpdatesRoom()
    {
        // Arrange
        var room = CreateRoom();

        _roomRepository.Setup(x => x.GetByIdAsync(room.Id)).ReturnsAsync(room);

        // Act
        var result = await _roomService.UpdateAsync(room.Id, "Updated Room", 20, 150m, new List<UpdateAdditionalServiceInput>());

        // Assert
        Assert.That(result, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(room.Name, Is.EqualTo("Updated Room"));
            Assert.That(room.Capacity, Is.EqualTo(20));
            Assert.That(room.HourlyRate, Is.EqualTo(150m));
        });

        _roomRepository.Verify(
            x => x.UpdateAsync(
                room,
                It.Is<IReadOnlyCollection<AdditionalService>>(services => services.Count == 0),
                It.Is<IReadOnlyCollection<AdditionalService>>(services => services.Count == 0)),
            Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WithAvailableServices_UpdatesAddsAndRemovesServices()
    {
        // Arrange
        var projectorId = Guid.NewGuid();
        var wifiId = Guid.NewGuid();

        var room = CreateRoom();

        room.AvailableServices =
        [
            new AdditionalService
        {
            Id = projectorId,
            RoomId = room.Id,
            Name = "Projector",
            Price = 500m
        },
        new AdditionalService
        {
            Id = wifiId,
            RoomId = room.Id,
            Name = "Wi-Fi",
            Price = 300m
        }
        ];

        _roomRepository.Setup(x => x.GetByIdAsync(room.Id))
            .ReturnsAsync(room);

        var services = new[]
        {
        new UpdateAdditionalServiceInput(
            projectorId,
            "Projector",
            600m),

        new UpdateAdditionalServiceInput(
            null,
            "Sound",
            700m)
        };

        // Act
        var result = await _roomService.UpdateAsync(
            room.Id,
            room.Name,
            room.Capacity,
            room.HourlyRate,
            services);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(room.AvailableServices, Has.Count.EqualTo(2));

        var projector = room.AvailableServices.Single(service => service.Id == projectorId);

        var sound = room.AvailableServices.Single(service => service.Name == "Sound");

        Assert.Multiple(() =>
        {
            // Existing service was updated
            Assert.That(projector.Name, Is.EqualTo("Projector"));
            Assert.That(projector.Price, Is.EqualTo(600m));

            // New service was created
            Assert.That(sound.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(sound.Price, Is.EqualTo(700m));

            // Omitted service was removed
            Assert.That(room.AvailableServices.Any(service => service.Id == wifiId), Is.False);
        });

        _roomRepository.Verify(
            x => x.UpdateAsync(
                room,
                It.Is<IReadOnlyCollection<AdditionalService>>(services =>
                    services.Count == 1 &&
                    services.Single().Name == "Sound" &&
                    services.Single().Price == 700m),
                It.Is<IReadOnlyCollection<AdditionalService>>(services =>
                    services.Count == 1 &&
                    services.Single().Id == wifiId)),
            Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenRoomDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        _roomRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Room?)null);

        // Act
        var result = await _roomService.UpdateAsync(id, "Room A", 10, 100m, new List<UpdateAdditionalServiceInput>());

        // Assert
        Assert.That(result, Is.False);

        _roomRepository.Verify(x => x.UpdateAsync(
            It.IsAny<Room>(),
            It.IsAny<IReadOnlyCollection<AdditionalService>>(),
            It.IsAny<IReadOnlyCollection<AdditionalService>>()),
            Times.Never);
    }

    [Test]
    public void UpdateAsync_WithServiceIdNotBelongingToRoom_ThrowsArgumentException()
    {
        // Arrange
        var room = CreateRoom();

        var existingService = new AdditionalService
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            Name = "Projector",
            Price = 500m
        };

        room.AvailableServices.Add(existingService);

        _roomRepository.Setup(x => x.GetByIdAsync(room.Id))
            .ReturnsAsync(room);

        var services = new[]
        {
            new UpdateAdditionalServiceInput(
                Guid.NewGuid(),
                "Sound",
                700m)
        };

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(() =>
            _roomService.UpdateAsync(
                room.Id,
                room.Name,
                room.Capacity,
                room.HourlyRate,
                services));

        _roomRepository.Verify(x => x.UpdateAsync(
            It.IsAny<Room>(),
            It.IsAny<IReadOnlyCollection<AdditionalService>>(),
            It.IsAny<IReadOnlyCollection<AdditionalService>>()),
            Times.Never);
    }

    [Test]
    public void UpdateAsync_WithEmptyServiceId_ThrowsArgumentException()
    {
        // Arrange
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room",
            Capacity = 10,
            HourlyRate = 100,
            AvailableServices =
            [
                new AdditionalService
            {
                Id = Guid.NewGuid(),
                Name = "Projector",
                Price = 500
            }
            ]
        };

        _roomRepository
            .Setup(repository => repository.GetByIdAsync(room.Id))
            .ReturnsAsync(room);

        var availableServices = new List<UpdateAdditionalServiceInput>
        {
            new(Guid.Empty, "Projector", 600)
        };

        // Act and Assert   
        var exception = Assert.ThrowsAsync<ArgumentException>(
            () => _roomService.UpdateAsync(
                room.Id,
                "Updated Room",
                20,
                150,
                availableServices));

        _roomRepository.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<Room>(),
                It.IsAny<IReadOnlyCollection<AdditionalService>>(),
                It.IsAny<IReadOnlyCollection<AdditionalService>>()),
            Times.Never);
    }

    [Test]
    public async Task DeleteAsync_WhenRoomExists_DeletesRoom()
    {
        // Arrange
        var room = CreateRoom();

        _roomRepository.Setup(x => x.GetByIdAsync(room.Id)).ReturnsAsync(room);

        // Act
        var result = await _roomService.DeleteAsync(room.Id);

        // Assert
        Assert.That(result, Is.True);

        _roomRepository.Verify(x => x.DeleteAsync(room), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WhenRoomDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        _roomRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Room?)null);

        // Act
        var result = await _roomService.DeleteAsync(id);

        // Assert
        Assert.That(result, Is.False);

        _roomRepository.Verify(x => x.DeleteAsync(It.IsAny<Room>()), Times.Never);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void CreateAsync_WithInvalidName_ThrowsArgumentException(string? name)
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await _roomService.CreateAsync(name!, 10, 100m, new List<AdditionalServiceInput>()));

        _roomRepository.Verify(x => x.AddAsync(It.IsAny<Room>()), Times.Never);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void CreateAsync_WithInvalidCapacity_ThrowsArgumentOutOfRangeException(int capacity)
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _roomService.CreateAsync("Room A", capacity, 100m, new List<AdditionalServiceInput>()));

        _roomRepository.Verify(x => x.AddAsync(It.IsAny<Room>()), Times.Never);
    }

    [Test]
    public void CreateAsync_WithNegativeHourlyRate_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _roomService.CreateAsync("Room A", 10, -1m, new List<AdditionalServiceInput>()));
        _roomRepository.Verify(x => x.AddAsync(It.IsAny<Room>()), Times.Never);
    }

    [Test]
    public void CreateAsync_WithEmptyServiceName_ThrowsArgumentException()
    {
        var services = new[]
        {
            new AdditionalServiceInput("", 500m)
        };

        Assert.ThrowsAsync<ArgumentException>(() => _roomService.CreateAsync("Room A", 10, 100m, services));
    }

    [Test]
    public void CreateAsync_WithNegativeServicePrice_ThrowsArgumentOutOfRangeException()
    {
        var services = new[]
        {
            new AdditionalServiceInput("Projector", -1m)
        };

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _roomService.CreateAsync("Room A", 10, 100m, services));
    }

    [Test]
    public async Task GetAvailableRoomsAsync_WithValidParameters_ReturnsAvailableRooms()
    {
        // Arrange
        var start = new DateTime(2026, 10, 1, 10, 0, 0);
        var end = new DateTime(2026, 10, 1, 14, 0, 0);
        const int capacity = 50;

        var rooms = new List<Room>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Room A",
                Capacity = 50,
                HourlyRate = 2000m
            }
        };

        _roomRepository
            .Setup(repository => repository.GetAvailableAsync(start, end, capacity))
            .ReturnsAsync(rooms);

        // Act
        var result = await _roomService.GetAvailableRoomsAsync(start, end, capacity);

        // Assert
        Assert.That(result, Is.SameAs(rooms));

        _roomRepository.Verify(repository => repository.GetAvailableAsync(start, end, capacity), Times.Once);
    }

    [Test]
    public void GetAvailableRoomsAsync_WhenCapacityIsZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var start = new DateTime(2026, 10, 1, 10, 0, 0);
        var end = new DateTime(2026, 10, 1, 14, 0, 0);

        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await _roomService.GetAvailableRoomsAsync(start, end, capacity: 0));

        _roomRepository.Verify(
            repository => repository.GetAvailableAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>()),
            Times.Never);
    }

    [Test]
    public void GetAvailableRoomsAsync_WhenStartIsAfterEnd_ThrowsArgumentException()
    {
        // Arrange
        var start = new DateTime(2026, 10, 1, 14, 0, 0);
        var end = new DateTime(2026, 10, 1, 10, 0, 0);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _roomService.GetAvailableRoomsAsync(start, end, capacity: 50));

        _roomRepository.Verify(
            repository => repository.GetAvailableAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>()),
            Times.Never);
    }

    [Test]
    public void GetAvailableRoomsAsync_WhenTimeIsNotOnFullHour_ThrowsArgumentException()
    {
        // Arrange
        var start = new DateTime(2026, 10, 1, 10, 30, 0);
        var end = new DateTime(2026, 10, 1, 14, 0, 0);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _roomService.GetAvailableRoomsAsync(
                start,
                end,
                capacity: 50));

        _roomRepository.Verify(
            repository => repository.GetAvailableAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>()),
            Times.Never);
    }

    [Test]
    public void GetAvailableRoomsAsync_WhenTimeIsOutsideBusinessHours_ThrowsArgumentException()
    {
        // Arrange
        var start = new DateTime(2026, 10, 1, 5, 0, 0);
        var end = new DateTime(2026, 10, 1, 10, 0, 0);

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _roomService.GetAvailableRoomsAsync(start, end, capacity: 50));

        _roomRepository.Verify(
            repository => repository.GetAvailableAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()),
            Times.Never);
    }

    private static Room CreateRoom(string name = "Room A")
    {
        return new Room
        {
            Id = Guid.NewGuid(),
            Name = name,
            Capacity = 10,
            HourlyRate = 100m
        };
    }
}

