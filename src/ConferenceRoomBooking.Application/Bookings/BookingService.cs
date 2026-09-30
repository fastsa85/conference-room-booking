using ConferenceRoomBooking.Application.Pricing;
using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Domain.Enums;


namespace ConferenceRoomBooking.Application.Bookings;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IPricingService _pricingService;
    private readonly BookingValidator _bookingValidator;

    public BookingService(IBookingRepository bookingRepository, IPricingService pricingService,  BookingValidator bookingValidator)
    {
        _bookingRepository = bookingRepository;
        _pricingService = pricingService;
        _bookingValidator = bookingValidator;
    }

    public async Task<Booking> CreateAsync(
        Guid roomId,
        DateTime start,
        DateTime end,
        IReadOnlyCollection<Guid> additionalServiceIds)
    {
        var validationErrors = _bookingValidator.Validate(start, end);

        if (validationErrors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", validationErrors));
        }

        var room = await _bookingRepository.GetRoomForBookingAsync(roomId);

        if (room is null)
        {
            throw new ArgumentException("Room does not exist.");
        }

        var hasOverlap = await _bookingRepository.HasOverlappingBookingAsync(roomId, start, end);

        if (hasOverlap)
        {
            throw new InvalidOperationException("Room is not available for the selected time.");
        }

        var selectedServices = room.AvailableServices
            .Where(service => additionalServiceIds.Contains(service.Id))
            .ToList();

        if (selectedServices.Count != additionalServiceIds.Count)
        {
            throw new ArgumentException("One or more selected services are not available for this room.");
        }

        var roomCost = _pricingService.CalculateRoomCost(room.HourlyRate, start, end);

        var servicesCost = selectedServices.Sum(service => service.Price);

        var bookingId = Guid.NewGuid();

        var booking = new Booking
        {
            Id = bookingId,
            RoomId = roomId,
            Start = start,
            End = end,
            Status = BookingStatus.Confirmed,
            TotalCost = roomCost + servicesCost,
            AdditionalServices = selectedServices
                .Select(service => new BookingAdditionalService
                {
                    Id = Guid.NewGuid(),
                    BookingId = bookingId,
                    AdditionalServiceId = service.Id,
                    Price = service.Price
                })
                .ToList()
        };

        await _bookingRepository.AddAsync(booking);

        return booking;
    }
}
