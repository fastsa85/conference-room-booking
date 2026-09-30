using ConferenceRoomBooking.Api.Models.Rooms;
using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Infrastructure.Persistence;
using ConferenceRoomBooking.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace ConferenceRoomBooking.IntegrationTests.Api;

[TestFixture]
[Category("ApiIntegration")]
public class RoomsControllerTests
{
    private const string RoomsEndpoint = "/api/rooms";
    private const string AvailableRoomsEndpoint = "/api/rooms/available";

    private ApiFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = new ApiFixture();
        await _fixture.InitializeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        await _fixture.ResetDatabaseAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _fixture.DisposeAsync();
    }

    // POST /api/rooms

    [Test]
    public async Task CreateRoom_WithValidRequest_ReturnsCreatedRoom()
    {
        // Arrange
        var request = new
        {
            Name = "Meeting Room A",
            Capacity = 10,
            HourlyRate = 100m
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync(RoomsEndpoint, request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var createdRoom = await response.Content.ReadFromJsonAsync<RoomResponse>();

        Assert.That(createdRoom, Is.Not.Null);

        var created = createdRoom!;

        Assert.Multiple(() =>
        {
            Assert.That(created.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(created.Name, Is.EqualTo(request.Name));
            Assert.That(created.Capacity, Is.EqualTo(request.Capacity));
            Assert.That(created.HourlyRate, Is.EqualTo(request.HourlyRate));

            Assert.That(response.Headers.Location, Is.Not.Null);
            Assert.That(response.Headers.Location!.AbsolutePath, Is.EqualTo($"{RoomsEndpoint}/{created.Id}"));
        });

        var persistedRoom = await _fixture.GetRoomAsync(created.Id);

        Assert.That(persistedRoom, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(persistedRoom!.Name, Is.EqualTo(request.Name));
            Assert.That(persistedRoom.Capacity, Is.EqualTo(request.Capacity));
            Assert.That(persistedRoom.HourlyRate, Is.EqualTo(request.HourlyRate));
        });
    }

    [Test]
    public async Task CreateRoom_WithAvailableServices_CreatesRoomAndServices()
    {
        // Arrange
        var request = new
        {
            Name = "Meeting Room A",
            Capacity = 10,
            HourlyRate = 100m,
            AvailableServices = new[]
            {
                new
                {
                    Name = "Projector",
                    Price = 500m
                },
                new
                {
                    Name = "Wi-Fi",
                    Price = 300m
                }
            }
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync(RoomsEndpoint, request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var createdRoom = await response.Content.ReadFromJsonAsync<RoomResponse>();

        Assert.That(createdRoom, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(createdRoom!.AvailableServices, Has.Count.EqualTo(2));

            Assert.That(
                createdRoom.AvailableServices.Any(service =>
                    service.Id != Guid.Empty &&
                    service.Name == "Projector" &&
                    service.Price == 500m),
                Is.True);

            Assert.That(
                createdRoom.AvailableServices.Any(service =>
                    service.Id != Guid.Empty &&
                    service.Name == "Wi-Fi" &&
                    service.Price == 300m),
                Is.True);
        });

        var persistedRoom = await _fixture.GetRoomAsync(createdRoom!.Id);

        Assert.That(persistedRoom, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(
                persistedRoom!.AvailableServices,
                Has.Count.EqualTo(2));

            Assert.That(
                persistedRoom.AvailableServices.Any(service =>
                    service.Name == "Projector" &&
                    service.Price == 500m &&
                    service.RoomId == persistedRoom.Id),
                Is.True);

            Assert.That(
                persistedRoom.AvailableServices.Any(service =>
                    service.Name == "Wi-Fi" &&
                    service.Price == 300m &&
                    service.RoomId == persistedRoom.Id),
                Is.True);
        });
    }

    [TestCase("", 10, 100)]
    [TestCase("Meeting Room A", 0, 100)]
    [TestCase("Meeting Room A", -1, 100)]
    [TestCase("Meeting Room A", 10, -1)]
    public async Task CreateRoom_WithInvalidRequest_ReturnsBadRequest(
    string name,
    int capacity,
    decimal hourlyRate)
    {
        // Arrange
        var request = new
        {
            Name = name,
            Capacity = capacity,
            HourlyRate = hourlyRate
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync(
            "/api/rooms",
            request);

        // Assert
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateRoom_WithWhitespaceName_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            Name = "   ",
            Capacity = 10,
            HourlyRate = 100m
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync(RoomsEndpoint, request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("", 500)]
    [TestCase("   ", 500)]
    [TestCase("Projector", -1)]
    public async Task CreateRoom_WithInvalidAvailableService_ReturnsBadRequest(string serviceName, decimal servicePrice)
    {
        var request = new
        {
            Name = "Meeting Room A",
            Capacity = 10,
            HourlyRate = 100m,
            AvailableServices = new[]
            {
                new
                {
                    Name = serviceName,
                    Price = servicePrice
                }
            }
        };

        var response = await _fixture.Client.PostAsJsonAsync(RoomsEndpoint, request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    // GET /api/rooms/{id}

    [Test]
    public async Task GetRoom_WhenRoomExists_ReturnsRoom()
    {
        // Arrange
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room A",
            Capacity = 10,
            HourlyRate = 100m
        };

        await _fixture.AddRoomAsync(room);

        // Act
        var response = await _fixture.Client.GetAsync($"{RoomsEndpoint}/{room.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var returnedRoom = await response.Content.ReadFromJsonAsync<RoomResponse>();

        Assert.That(returnedRoom, Is.Not.Null);

        var returned = returnedRoom!;

        Assert.Multiple(() =>
        {
            Assert.That(returned.Id, Is.EqualTo(room.Id));
            Assert.That(returned.Name, Is.EqualTo(room.Name));
            Assert.That(returned.Capacity, Is.EqualTo(room.Capacity));
            Assert.That(returned.HourlyRate, Is.EqualTo(room.HourlyRate));
        });
    }

    [Test]
    public async Task GetRoom_WhenRoomDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act
        var response = await _fixture.Client.GetAsync($"{RoomsEndpoint}/{roomId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // GET /api/rooms

    [Test]
    public async Task GetAllRooms_WhenRoomsExist_ReturnsRooms()
    {
        // Arrange
        var room1 = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room A",
            Capacity = 10,
            HourlyRate = 100m
        };

        var room2 = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room B",
            Capacity = 20,
            HourlyRate = 150m
        };

        await _fixture.AddRoomAsync(room1);
        await _fixture.AddRoomAsync(room2);

        // Act
        var response = await _fixture.Client.GetAsync(RoomsEndpoint);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var rooms = await response.Content.ReadFromJsonAsync<List<RoomResponse>>();

        Assert.That(rooms, Is.Not.Null);
        Assert.That(rooms, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(
                rooms!.Any(room =>
                    room.Id == room1.Id &&
                    room.Name == room1.Name &&
                    room.Capacity == room1.Capacity &&
                    room.HourlyRate == room1.HourlyRate),
                Is.True);

            Assert.That(
                rooms.Any(room =>
                    room.Id == room2.Id &&
                    room.Name == room2.Name &&
                    room.Capacity == room2.Capacity &&
                    room.HourlyRate == room2.HourlyRate),
                Is.True);
        });
    }

    [Test]
    public async Task GetAllRooms_WhenNoRoomsExist_ReturnsEmptyCollection()
    {
        // Act
        var response = await _fixture.Client.GetAsync(RoomsEndpoint);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var rooms = await response.Content.ReadFromJsonAsync<List<RoomResponse>>();

        Assert.That(rooms, Is.Not.Null);
        Assert.That(rooms, Is.Empty);
    }

    // PUT /api/rooms/{id}

    [Test]
    public async Task UpdateRoom_WhenRoomExists_UpdatesRoom()
    {
        // Arrange
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room A",
            Capacity = 10,
            HourlyRate = 100m
        };

        await _fixture.AddRoomAsync(room);

        var request = new
        {
            Name = "Updated Meeting Room",
            Capacity = 20,
            HourlyRate = 150m
        };

        // Act
        var response = await _fixture.Client.PutAsJsonAsync($"{RoomsEndpoint}/{room.Id}", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var updatedRoom = await _fixture.GetRoomAsync(room.Id);

        Assert.That(updatedRoom, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(updatedRoom!.Id, Is.EqualTo(room.Id));
            Assert.That(updatedRoom.Name, Is.EqualTo(request.Name));
            Assert.That(updatedRoom.Capacity, Is.EqualTo(request.Capacity));
            Assert.That(updatedRoom.HourlyRate, Is.EqualTo(request.HourlyRate));
        });
    }

    [Test]
    public async Task UpdateRoom_WithAvailableServices_UpdatesAddsAndRemovesServices()
    {
        // Arrange
        var projectorId = Guid.NewGuid();
        var wifiId = Guid.NewGuid();

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
                    Id = projectorId,
                    Name = "Projector",
                    Price = 500m
                },
                new AdditionalService
                {
                    Id = wifiId,
                    Name = "Wi-Fi",
                    Price = 300m
                }
            ]
        };

        await _fixture.AddRoomAsync(room);

        var request = new
        {
            Name = "Updated Room",
            Capacity = 20,
            HourlyRate = 150m,
            AvailableServices = new object[]
            {
                new
                {
                    Id = (Guid?)projectorId,
                    Name = "Projector",
                    Price = 600m
                },
                new
                {
                    Id = (Guid?)null,
                    Name = "Sound",
                    Price = 700m
                }
            }
        };

        // Act
        var response = await _fixture.Client.PutAsJsonAsync($"{RoomsEndpoint}/{room.Id}", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var persistedRoom = await _fixture.GetRoomAsync(room.Id);

        Assert.That(persistedRoom, Is.Not.Null);
        Assert.That(persistedRoom!.AvailableServices, Has.Count.EqualTo(2));

        var projector = persistedRoom.AvailableServices.Single(service => service.Id == projectorId);
        var sound = persistedRoom.AvailableServices.Single(service => service.Name == "Sound");

        Assert.Multiple(() =>
        {
            Assert.That(persistedRoom.Name, Is.EqualTo("Updated Room"));
            Assert.That(persistedRoom.Capacity, Is.EqualTo(20));
            Assert.That(persistedRoom.HourlyRate, Is.EqualTo(150m));

            // Existing service updated
            Assert.That(projector.Price, Is.EqualTo(600m));

            // New service inserted
            Assert.That(sound.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(sound.Price, Is.EqualTo(700m));
            Assert.That(sound.RoomId, Is.EqualTo(room.Id));

            // Omitted service deleted
            Assert.That(persistedRoom.AvailableServices.Any(service => service.Id == wifiId), Is.False);
        });
    }

    [Test]
    public async Task UpdateRoom_WhenRoomDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        var request = new
        {
            Name = "Updated Meeting Room",
            Capacity = 20,
            HourlyRate = 150m
        };

        // Act
        var response = await _fixture.Client.PutAsJsonAsync($"{RoomsEndpoint}/{roomId}", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [TestCase("", 10, 100)]
    [TestCase("   ", 10, 100)]
    [TestCase("Meeting Room A", 0, 100)]
    [TestCase("Meeting Room A", -1, 100)]
    [TestCase("Meeting Room A", 10, -1)]
    public async Task UpdateRoom_WithInvalidRequest_ReturnsBadRequest(string name, int capacity, decimal hourlyRate)
    {
        // Arrange
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Original Room",
            Capacity = 10,
            HourlyRate = 100m
        };

        await _fixture.AddRoomAsync(room);

        var request = new
        {
            Name = name,
            Capacity = capacity,
            HourlyRate = hourlyRate
        };

        // Act
        var response = await _fixture.Client.PutAsJsonAsync($"{RoomsEndpoint}/{room.Id}", request);

        // Assert
        Assert.That(response.StatusCode,Is.EqualTo(HttpStatusCode.BadRequest));

        var persistedRoom = await _fixture.GetRoomAsync(room.Id);

        Assert.That(persistedRoom, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(persistedRoom!.Name, Is.EqualTo(room.Name));
            Assert.That(persistedRoom.Capacity, Is.EqualTo(room.Capacity));
            Assert.That(persistedRoom.HourlyRate, Is.EqualTo(room.HourlyRate));
        });
    }

    [Test]
    public async Task UpdateRoom_WithWhitespaceName_ReturnsBadRequest()
    {
        // Arrange
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Original Room",
            Capacity = 10,
            HourlyRate = 100m
        };

        await _fixture.AddRoomAsync(room);

        var request = new
        {
            Name = "   ",
            Capacity = 10,
            HourlyRate = 100m
        };

        // Act
        var response = await _fixture.Client.PutAsJsonAsync($"{RoomsEndpoint}/{room.Id}", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var persistedRoom = await _fixture.GetRoomAsync(room.Id);

        Assert.That(persistedRoom, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(persistedRoom!.Name, Is.EqualTo(room.Name));
            Assert.That(persistedRoom.Capacity, Is.EqualTo(room.Capacity));
            Assert.That(persistedRoom.HourlyRate, Is.EqualTo(room.HourlyRate));
        });
    }

    // DELETE /api/rooms/{id}

    [Test]
    public async Task DeleteRoom_WhenRoomExists_DeletesRoom()
    {
        // Arrange
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room A",
            Capacity = 10,
            HourlyRate = 100m
        };

        await _fixture.AddRoomAsync(room);

        // Act
        var response = await _fixture.Client.DeleteAsync($"{RoomsEndpoint}/{room.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var deletedRoom = await _fixture.GetRoomAsync(room.Id);

        Assert.That(deletedRoom, Is.Null);
    }

    [Test]
    public async Task DeleteRoom_WhenRoomDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act
        var response = await _fixture.Client.DeleteAsync($"{RoomsEndpoint}/{roomId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // GET /api/rooms/available

    [Test]
    public async Task GetAvailableRooms_WhenRoomIsAvailable_ReturnsRoom()
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
                }
            ]
        };

        await _fixture.AddRoomAsync(room);

        var start = new DateTime(2026, 10, 1, 10, 0, 0);
        var end = new DateTime(2026, 10, 1, 14, 0, 0);

        // Act
        var response = await _fixture.Client.GetAsync($"{AvailableRoomsEndpoint}?start={start:yyyy-MM-ddTHH:mm:ss}&end={end:yyyy-MM-ddTHH:mm:ss}&capacity=50");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var rooms = await response.Content.ReadFromJsonAsync<List<RoomResponse>>();

        Assert.That(rooms, Is.Not.Null);
        Assert.That(rooms, Has.Count.EqualTo(1));

        var returnedRoom = rooms!.Single();

        Assert.Multiple(() =>
        {
            Assert.That(returnedRoom.Id, Is.EqualTo(room.Id));
            Assert.That(returnedRoom.Name, Is.EqualTo(room.Name));
            Assert.That(returnedRoom.Capacity, Is.EqualTo(room.Capacity));
            Assert.That(returnedRoom.HourlyRate, Is.EqualTo(room.HourlyRate));

            Assert.That(returnedRoom.AvailableServices, Has.Count.EqualTo(1));

            Assert.That(returnedRoom.AvailableServices.Single().Name, Is.EqualTo("Projector"));

            Assert.That(returnedRoom.AvailableServices.Single().Price, Is.EqualTo(500m));
        });
    }

    [Test]
    public async Task GetAvailableRooms_WithInvalidCapacity_ReturnsBadRequest()
    {
        // Arrange
        var start = new DateTime(2026, 10, 1, 10, 0, 0);
        var end = new DateTime(2026, 10, 1, 14, 0, 0);

        // Act
        var response = await _fixture.Client.GetAsync($"{AvailableRoomsEndpoint}?start={start:yyyy-MM-ddTHH:mm:ss}&end={end:yyyy-MM-ddTHH:mm:ss}&capacity=0"); // !

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
