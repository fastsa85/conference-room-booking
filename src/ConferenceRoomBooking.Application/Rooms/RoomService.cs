using ConferenceRoomBooking.Domain.Entities;

namespace ConferenceRoomBooking.Application.Rooms
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;

        public RoomService(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public Task<IReadOnlyCollection<Room>> GetAllAsync()
        {
            return _roomRepository.GetAllAsync();
        }

        public Task<Room?> GetByIdAsync(Guid id)
        {
            return _roomRepository.GetByIdAsync(id);
        }

        public async Task<Room> CreateAsync(string name, int capacity, decimal hourlyRate)
        {
            Validate(name, capacity, hourlyRate);

            var room = new Room
            {
                Id = Guid.NewGuid(),
                Name = name,
                Capacity = capacity,
                HourlyRate = hourlyRate
            };

            await _roomRepository.AddAsync(room);

            return room;
        }

        public async Task<bool> UpdateAsync(Guid id, string name, int capacity, decimal hourlyRate)
        {
            Validate(name, capacity, hourlyRate);

            var room = await _roomRepository.GetByIdAsync(id);

            if (room is null)
            {
                return false;
            }

            room.Name = name;
            room.Capacity = capacity;
            room.HourlyRate = hourlyRate;

            await _roomRepository.UpdateAsync(room);

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var room = await _roomRepository.GetByIdAsync(id);

            if (room is null)
            {
                return false;
            }

            await _roomRepository.DeleteAsync(room);

            return true;
        }

        private static void Validate(string name, int capacity, decimal hourlyRate)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Room name is required.", nameof(name));
            }

            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Room capacity must be greater than zero.");
            }

            if (hourlyRate < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hourlyRate), "Hourly rate cannot be negative.");
            }
        }
    }
}
