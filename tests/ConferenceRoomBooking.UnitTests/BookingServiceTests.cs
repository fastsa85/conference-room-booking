using ConferenceRoomBooking.Application.Bookings;
using ConferenceRoomBooking.Application.Pricing;
using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Domain.Enums;
using Moq;

namespace ConferenceRoomBooking.UnitTests;

[TestFixture]
public class BookingServiceTests
{
    private Mock<IBookingRepository> _bookingRepositoryMock = null!;
    private Mock<IPricingService> _pricingServiceMock = null!;
    private BookingValidator _bookingValidator = null!;
    private BookingService _bookingService = null!;

    private readonly DateTime _now = new(2026, 10, 1, 8, 0, 0);

    [SetUp]
    public void SetUp()
    {
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _pricingServiceMock = new Mock<IPricingService>();

        _bookingValidator = new BookingValidator(() => _now);

        _bookingService = new BookingService(_bookingRepositoryMock.Object, _pricingServiceMock.Object, _bookingValidator);
    }

    [Test]
    public async Task CreateAsync_WhenBookingIsValid_CreatesConfirmedBooking()
    {
        var room = CreateRoom();

        _bookingRepositoryMock.Setup(repository => repository.GetRoomForBookingAsync(room.Id))
            .ReturnsAsync(room);

        _bookingRepositoryMock.Setup(repository => repository.HasOverlappingBookingAsync(room.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        _pricingServiceMock.Setup(service => service.CalculateRoomCost(room.HourlyRate, It.IsAny<DateTime>(),  It.IsAny<DateTime>()))
            .Returns(2000m);

        var start = new DateTime(2026, 10, 1, 10, 0, 0);
        var end = new DateTime(2026, 10, 1, 12, 0, 0);

        var booking = await _bookingService.CreateAsync(room.Id, start, end, []);

        Assert.Multiple(() =>
        {
            Assert.That(booking.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(booking.RoomId, Is.EqualTo(room.Id));
            Assert.That(booking.Start, Is.EqualTo(start));
            Assert.That(booking.End, Is.EqualTo(end));
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Confirmed));
            Assert.That(booking.TotalCost, Is.EqualTo(2000m));
            Assert.That(booking.AdditionalServices, Is.Empty);
        });

        _bookingRepositoryMock.Verify(repository => repository.AddAsync(booking), Times.Once);
    }

    [Test]
    public void CreateAsync_WhenBookingTimeIsInvalid_ThrowsArgumentException()
    {
        var start = new DateTime(2026, 10, 1, 12, 0, 0);
        var end = new DateTime(2026, 10, 1, 10, 0, 0);

        Assert.ThrowsAsync<ArgumentException>(() =>_bookingService.CreateAsync(Guid.NewGuid(), start, end, []));

        _bookingRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Test]
    public void CreateAsync_WhenRoomDoesNotExist_ThrowsArgumentException()
    {
        var roomId = Guid.NewGuid();

        _bookingRepositoryMock.Setup(repository => repository.GetRoomForBookingAsync(roomId))
            .ReturnsAsync((Room?)null);

        Assert.ThrowsAsync<ArgumentException>(() => _bookingService.CreateAsync(roomId, new DateTime(2026, 10, 1, 10, 0, 0), new DateTime(2026, 10, 1, 12, 0, 0), []));

        _bookingRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Test]
    public void CreateAsync_WhenRoomHasOverlappingBooking_ThrowsInvalidOperationException()
    {
        var room = CreateRoom();

        _bookingRepositoryMock.Setup(repository => repository.GetRoomForBookingAsync(room.Id)).ReturnsAsync(room);

        _bookingRepositoryMock.Setup(repository => repository.HasOverlappingBookingAsync(room.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        Assert.ThrowsAsync<InvalidOperationException>(() =>_bookingService.CreateAsync(room.Id, new DateTime(2026, 10, 1, 10, 0, 0), new DateTime(2026, 10, 1, 12, 0, 0), []));

        _bookingRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Test]
    public void CreateAsync_WhenSelectedServiceDoesNotBelongToRoom_ThrowsArgumentException()
    {
        var room = CreateRoom();

        _bookingRepositoryMock.Setup(repository => repository.GetRoomForBookingAsync(room.Id)).ReturnsAsync(room);

        _bookingRepositoryMock.Setup(repository => repository.HasOverlappingBookingAsync(room.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        Assert.ThrowsAsync<ArgumentException>(() =>_bookingService.CreateAsync(room.Id, new DateTime(2026, 10, 1, 10, 0, 0), new DateTime(2026, 10, 1, 12, 0, 0), [Guid.NewGuid()]));

        _bookingRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Test]
    public async Task CreateAsync_WithAdditionalServices_IncludesServicesInTotalCost()
    {
        var projector = new AdditionalService
        {
            Id = Guid.NewGuid(),
            Name = "Projector",
            Price = 200m
        };

        var catering = new AdditionalService
        {
            Id = Guid.NewGuid(),
            Name = "Catering",
            Price = 500m
        };

        var room = CreateRoom();

        room.AvailableServices.Add(projector);
        room.AvailableServices.Add(catering);

        _bookingRepositoryMock.Setup(repository => repository.GetRoomForBookingAsync(room.Id))
            .ReturnsAsync(room);

        _bookingRepositoryMock.Setup(repository => repository.HasOverlappingBookingAsync(room.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        _pricingServiceMock.Setup(service => service.CalculateRoomCost(room.HourlyRate, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(2000m);

        var booking = await _bookingService.CreateAsync(room.Id, new DateTime(2026, 10, 1, 10, 0, 0), new DateTime(2026, 10, 1, 12, 0, 0), [projector.Id, catering.Id]);

        Assert.Multiple(() =>
        {
            Assert.That(booking.TotalCost, Is.EqualTo(2700m));
            Assert.That(booking.AdditionalServices, Has.Count.EqualTo(2));

            Assert.That(booking.AdditionalServices.Single(x => x.AdditionalServiceId == projector.Id).Price, Is.EqualTo(200m));

            Assert.That(booking.AdditionalServices.Single(x => x.AdditionalServiceId == catering.Id).Price, Is.EqualTo(500m));
        });

        _bookingRepositoryMock.Verify(repository => repository.AddAsync(booking), Times.Once);
    }

    [Test]
    public void CreateAsync_WithDuplicateServiceIds_ThrowsArgumentException()
    {
        var projector = new AdditionalService
        {
            Id = Guid.NewGuid(),
            Name = "Projector",
            Price = 200m
        };

        var room = CreateRoom();
        room.AvailableServices.Add(projector);

        _bookingRepositoryMock.Setup(repository => repository.GetRoomForBookingAsync(room.Id))
            .ReturnsAsync(room);

        _bookingRepositoryMock.Setup(repository => repository.HasOverlappingBookingAsync(room.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        Assert.ThrowsAsync<ArgumentException>(() =>_bookingService.CreateAsync(room.Id, new DateTime(2026, 10, 1, 10, 0, 0), new DateTime(2026, 10, 1, 12, 0, 0), [projector.Id, projector.Id]));

        _bookingRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    private static Room CreateRoom()
    {
        return new Room
        {
            Id = Guid.NewGuid(),
            Name = "Conference Room",
            Capacity = 10,
            HourlyRate = 1000m
        };
    }
}
