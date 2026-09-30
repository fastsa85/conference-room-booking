using ConferenceRoomBooking.Application.Bookings;
using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.Infrastructure.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _dbContext;

    public BookingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Room?> GetRoomForBookingAsync(Guid roomId)
    {
        return await _dbContext.Rooms
            .Include(room => room.AvailableServices)
            .SingleOrDefaultAsync(room => room.Id == roomId);
    }

    public async Task<bool> HasOverlappingBookingAsync(
        Guid roomId,
        DateTime start,
        DateTime end)
    {
        return await _dbContext.Bookings
            .AnyAsync(booking =>
                booking.RoomId == roomId &&
                booking.Status == BookingStatus.Confirmed &&
                booking.Start < end &&
                booking.End > start);
    }

    public async Task AddAsync(Booking booking)
    {
        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync();
    }
}
