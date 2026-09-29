using ConferenceRoomBooking.Application.Rooms;
using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.Infrastructure.Persistence.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _dbContext;

    public RoomRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Room>> GetAllAsync()
    {
        return await _dbContext.Rooms
            .Include(room => room.AvailableServices)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Room?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Rooms
            .Include(room => room.AvailableServices)
            .SingleOrDefaultAsync(room => room.Id == id);
    }

    public async Task AddAsync(Room room)
    {
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(
    Room room,
    IReadOnlyCollection<AdditionalService>? servicesToAdd,
    IReadOnlyCollection<AdditionalService>? servicesToRemove)
    {
        if (servicesToRemove is not null)
        {
            _dbContext.AdditionalServices.RemoveRange(servicesToRemove);
        }

        if (servicesToAdd is not null)
        {
            _dbContext.AdditionalServices.AddRange(servicesToAdd);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Room>> GetAvailableAsync(DateTime start, DateTime end, int capacity)
    {
        return await _dbContext.Rooms.Where(room =>
            room.Capacity >= capacity &&
            !room.Bookings.Any(booking =>
                booking.Status == BookingStatus.Confirmed &&
                booking.Start < end &&
                booking.End > start))
        .Include(room => room.AvailableServices)
        .ToListAsync();
    }

    public async Task DeleteAsync(Room room)
    {
        _dbContext.Rooms.Remove(room);
        await _dbContext.SaveChangesAsync();
    }
}