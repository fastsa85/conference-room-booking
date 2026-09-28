using ConferenceRoomBooking.Domain.Entities;

namespace ConferenceRoomBooking.Application.Rooms;

public interface IRoomService
{
    Task<IReadOnlyCollection<Room>> GetAllAsync();

    Task<Room?> GetByIdAsync(Guid id);

    Task<Room> CreateAsync(string name, int capacity, decimal hourlyRate, IReadOnlyCollection<AdditionalServiceInput> availableServices);

    Task<bool> UpdateAsync(Guid id, string name, int capacity, decimal hourlyRate, IReadOnlyCollection<UpdateAdditionalServiceInput> availableServices);

    Task<bool> DeleteAsync(Guid id);
}
