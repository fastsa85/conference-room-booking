using ConferenceRoomBooking.Application.Rooms;
using ConferenceRoomBooking.Domain.Entities;
using Moq;

namespace ConferenceRoomBooking.UnitTests.Application.Rooms;

[TestFixture]
public class RoomServiceTests
{
    private Mock<IRoomRepository> _repository = null!;
    private RoomService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IRoomRepository>();
        _service = new RoomService(_repository.Object);
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

        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync(rooms);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.That(result, Is.SameAs(rooms));
    }

    [Test]
    public async Task GetAllAsync_WhenNoRoomsExist_ReturnsEmptyCollection()
    {
        // Arrange
        var rooms = new List<Room>();

        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync(rooms);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetByIdAsync_WhenRoomExists_ReturnsRoom()
    {
        // Arrange
        var room = CreateRoom();

        _repository.Setup(x => x.GetByIdAsync(room.Id)).ReturnsAsync(room);

        // Act
        var result = await _service.GetByIdAsync(room.Id);

        // Assert
        Assert.That(result, Is.SameAs(room));
    }

    [Test]
    public async Task GetByIdAsync_WhenRoomDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Room?)null);

        // Act
        var result = await _service.GetByIdAsync(id);

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

        // Act
        var result = await _service.CreateAsync(name, capacity, hourlyRate);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.Capacity, Is.EqualTo(capacity));
            Assert.That(result.HourlyRate, Is.EqualTo(hourlyRate));
        });

        _repository.Verify(
            x => x.AddAsync(It.Is<Room>(room =>
                room.Id == result.Id &&
                room.Name == name &&
                room.Capacity == capacity &&
                room.HourlyRate == hourlyRate)),
            Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenRoomExists_UpdatesRoom()
    {
        // Arrange
        var room = CreateRoom();

        _repository.Setup(x => x.GetByIdAsync(room.Id)).ReturnsAsync(room);

        // Act
        var result = await _service.UpdateAsync(room.Id, "Updated Room", 20, 150m);

        // Assert
        Assert.That(result, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(room.Name, Is.EqualTo("Updated Room"));
            Assert.That(room.Capacity, Is.EqualTo(20));
            Assert.That(room.HourlyRate, Is.EqualTo(150m));
        });

        _repository.Verify(x => x.UpdateAsync(room), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenRoomDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Room?)null);

        // Act
        var result = await _service.UpdateAsync(id, "Room A", 10, 100m);

        // Assert
        Assert.That(result, Is.False);

        _repository.Verify(x => x.UpdateAsync(It.IsAny<Room>()), Times.Never);
    }

    [Test]
    public async Task DeleteAsync_WhenRoomExists_DeletesRoom()
    {
        // Arrange
        var room = CreateRoom();

        _repository.Setup(x => x.GetByIdAsync(room.Id)).ReturnsAsync(room);

        // Act
        var result = await _service.DeleteAsync(room.Id);

        // Assert
        Assert.That(result, Is.True);

        _repository.Verify(x => x.DeleteAsync(room), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WhenRoomDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Room?)null);

        // Act
        var result = await _service.DeleteAsync(id);

        // Assert
        Assert.That(result, Is.False);

        _repository.Verify(x => x.DeleteAsync(It.IsAny<Room>()), Times.Never);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void CreateAsync_WithInvalidName_ThrowsArgumentException(string? name)
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await _service.CreateAsync(name!, 10, 100m));

        _repository.Verify(x => x.AddAsync(It.IsAny<Room>()), Times.Never);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void CreateAsync_WithInvalidCapacity_ThrowsArgumentOutOfRangeException(int capacity)
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _service.CreateAsync("Room A", capacity, 100m));

        _repository.Verify(x => x.AddAsync(It.IsAny<Room>()), Times.Never);
    }

    [Test]
    public void CreateAsync_WithNegativeHourlyRate_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _service.CreateAsync("Room A", 10, -1m));
        _repository.Verify(x => x.AddAsync(It.IsAny<Room>()), Times.Never);
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

