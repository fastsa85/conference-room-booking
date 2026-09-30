using ConferenceRoomBooking.Domain.Entities;

namespace ConferenceRoomBooking.Application.Bookings;

public interface IBookingRepository
{
    Task<Room?> GetRoomForBookingAsync(Guid roomId);

    Task<bool> HasOverlappingBookingAsync(
        Guid roomId,
        DateTime start,
        DateTime end);

    Task AddAsync(Booking booking);
}
