using ConferenceRoomBooking.Api.Models.Bookings;
using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Domain.Enums;
using System.Net;
using System.Net.Http.Json;

namespace ConferenceRoomBooking.IntegrationTests.Api;

[TestFixture]
[Category("ApiIntegration")]
public class BookingsControllerTests
{
    private static DateTime FutureBookingDate => DateTime.Today.AddDays(1);

    private const string BookingsEndpoint = "/api/bookings";
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

    // POST /api/bookings

    [Test]
    public async Task CreateBooking_WithValidRequest_ReturnsCreatedBooking()
    {
        // Arrange
        var room = CreateRoom();

        await _fixture.AddRoomAsync(room);

        var request = new
        {
            RoomId = room.Id,
            Start = FutureBookingDate.AddHours(10),
            End = FutureBookingDate.AddHours(12),
            AdditionalServiceIds = Array.Empty<Guid>()
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync(BookingsEndpoint, request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var booking = await response.Content.ReadFromJsonAsync<BookingResponse>();

        Assert.That(booking, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(booking!.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(booking.RoomId, Is.EqualTo(room.Id));
            Assert.That(booking.Start, Is.EqualTo(request.Start));
            Assert.That(booking.End, Is.EqualTo(request.End));
            Assert.That(booking.Status, Is.EqualTo("Confirmed"));
            Assert.That(booking.TotalCost, Is.EqualTo(200m));
            Assert.That(booking.AdditionalServices, Is.Empty);
        });
    }

    [Test]
    public async Task CreateBooking_WithAdditionalServices_ReturnsCalculatedTotalCost()
    {
        // Arrange
        var projector = new AdditionalService
        {
            Id = Guid.NewGuid(),
            Name = "Projector",
            Price = 50m
        };

        var catering = new AdditionalService
        {
            Id = Guid.NewGuid(),
            Name = "Catering",
            Price = 100m
        };

        var room = CreateRoom();

        room.AvailableServices.Add(projector);
        room.AvailableServices.Add(catering);

        await _fixture.AddRoomAsync(room);

        var request = new
        {
            RoomId = room.Id,
            Start = FutureBookingDate.AddHours(10),
            End = FutureBookingDate.AddHours(12),
            AdditionalServiceIds = new[]
            {
                projector.Id,
                catering.Id
            }
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync(BookingsEndpoint, request);

        // Assert
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Created));

        var booking =
            await response.Content.ReadFromJsonAsync<BookingResponse>();

        Assert.That(booking, Is.Not.Null);

        Assert.Multiple(() =>
        {
            // Room:
            // 10:00-12:00 = 2 * 100 = 200
            //
            // Services:
            // Projector = 50
            // Catering = 100
            //
            // Total = 350

            Assert.That(booking!.TotalCost, Is.EqualTo(350m));

            Assert.That(
                booking.AdditionalServices,
                Has.Count.EqualTo(2));

            Assert.That(
                booking.AdditionalServices.Any(service =>
                    service.AdditionalServiceId == projector.Id &&
                    service.Price == 50m),
                Is.True);

            Assert.That(
                booking.AdditionalServices.Any(service =>
                    service.AdditionalServiceId == catering.Id &&
                    service.Price == 100m),
                Is.True);
        });

        var persistedBooking = await _fixture.GetBookingAsync(booking!.Id);

        Assert.That(persistedBooking, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(persistedBooking!.Status, Is.EqualTo(BookingStatus.Confirmed));

            Assert.That(persistedBooking.TotalCost, Is.EqualTo(350m));

            Assert.That(persistedBooking.AdditionalServices, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task CreateBooking_WhenRoomDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            RoomId = Guid.NewGuid(),
            Start = new DateTime(2026, 10, 1, 10, 0, 0),
            End = new DateTime(2026, 10, 1, 12, 0, 0),
            AdditionalServiceIds = Array.Empty<Guid>()
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync(BookingsEndpoint, request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateBooking_WhenSelectedServiceDoesNotBelongToRoom_ReturnsBadRequest()
    {
        // Arrange
        var room = CreateRoom();

        await _fixture.AddRoomAsync(room);

        var request = new
        {
            RoomId = room.Id,
            Start = new DateTime(2026, 10, 1, 10, 0, 0),
            End = new DateTime(2026, 10, 1, 12, 0, 0),
            AdditionalServiceIds = new[]
            {
                Guid.NewGuid()
            }
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync(BookingsEndpoint, request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateBooking_WhenBookingOverlapsExistingBooking_ReturnsConflict()
    {
        // Arrange
        var room = CreateRoom();

        room.Bookings.Add(new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            Start = FutureBookingDate.AddHours(10),
            End = FutureBookingDate.AddHours(12),
            Status = BookingStatus.Confirmed,
            TotalCost = 200m
        });

        await _fixture.AddRoomAsync(room);

        var request = new
        {
            RoomId = room.Id,
            Start = FutureBookingDate.AddHours(11),
            End = FutureBookingDate.AddHours(13),
            AdditionalServiceIds = Array.Empty<Guid>()
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync(BookingsEndpoint, request);

        // Assert
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task CreateBooking_WhenBookingTouchesExistingBookingBoundary_ReturnsCreated()
    {
        // Arrange
        var room = CreateRoom();

        room.Bookings.Add(new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            Start = FutureBookingDate.AddHours(10),
            End = FutureBookingDate.AddHours(12),
            Status = BookingStatus.Confirmed,
            TotalCost = 200m
        });

        await _fixture.AddRoomAsync(room);

        var request = new
        {
            RoomId = room.Id,
            Start = FutureBookingDate.AddHours(12),
            End = FutureBookingDate.AddHours(14),
            AdditionalServiceIds = Array.Empty<Guid>()
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync(BookingsEndpoint, request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task CreateBooking_WithInvalidTime_ReturnsBadRequest()
    {
        // Arrange
        var room = CreateRoom();

        await _fixture.AddRoomAsync(room);

        var request = new
        {
            RoomId = room.Id,
            Start = new DateTime(2026, 10, 1, 12, 0, 0),
            End = new DateTime(2026, 10, 1, 10, 0, 0),
            AdditionalServiceIds = Array.Empty<Guid>()
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync(BookingsEndpoint, request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private static Room CreateRoom()
    {
        return new Room
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Room A",
            Capacity = 10,
            HourlyRate = 100m
        };
    }
}