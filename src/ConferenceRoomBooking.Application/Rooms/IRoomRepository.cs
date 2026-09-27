using ConferenceRoomBooking.Domain.Entities;

namespace ConferenceRoomBooking.Application.Rooms
{
    public interface IRoomRepository
    {
        Task<IReadOnlyCollection<Room>> GetAllAsync();
        Task<Room?> GetByIdAsync(Guid id);
        Task AddAsync(Room room);
        Task UpdateAsync(Room room);
        Task DeleteAsync(Room room);
    }
}
