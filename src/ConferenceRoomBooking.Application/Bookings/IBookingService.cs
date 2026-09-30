using ConferenceRoomBooking.Domain.Entities;

namespace ConferenceRoomBooking.Application.Bookings;

public interface IBookingService
{
    Task<Booking> CreateAsync(
        Guid roomId,
        DateTime start,
        DateTime end,
        IReadOnlyCollection<Guid> additionalServiceIds);
}