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

        public async Task<Room> CreateAsync(string name, int capacity, decimal hourlyRate, IReadOnlyCollection<AdditionalServiceInput> availableServices)
        {
            ValidateRoomProperties(name, capacity, hourlyRate);
            ValidateAvailableServices(availableServices);

            var room = new Room
            {
                Id = Guid.NewGuid(),
                Name = name,
                Capacity = capacity,
                HourlyRate = hourlyRate,
                AvailableServices = availableServices.Select(service =>
                    new AdditionalService
                    {
                        Id = Guid.NewGuid(),
                        Name = service.Name,
                        Price = service.Price
                    })
                .ToList()
            };

            await _roomRepository.AddAsync(room);

            return room;
        }

        public async Task<bool> UpdateAsync(Guid id, string name, int capacity, decimal hourlyRate, IReadOnlyCollection<UpdateAdditionalServiceInput> availableServices)
        {
            ValidateRoomProperties(name, capacity, hourlyRate);
            ValidateAvailableServices(availableServices);

            var room = await _roomRepository.GetByIdAsync(id);

            if (room is null)
            {
                return false;
            }

            var requestedExistingIds = GetRequestedExistingServiceIds(availableServices);

            ValidateServiceIds(room, requestedExistingIds);

            // mutate only after validation to avoid leaving the room in an inconsistent state
            room.Name = name;
            room.Capacity = capacity;
            room.HourlyRate = hourlyRate;

            var servicesToRemove = RemoveMissingServices(room, requestedExistingIds);
            var servicesToAdd = UpdateAvailableServices(room, availableServices);

            await _roomRepository.UpdateAsync(room, servicesToAdd, servicesToRemove);

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

        public Task<IReadOnlyCollection<Room>> GetAvailableRoomsAsync(DateTime start, DateTime end, int capacity)
        {
            ValidateAvailabilitySearch(start, end, capacity);

            return _roomRepository.GetAvailableAsync(start, end, capacity);
        }

        private static HashSet<Guid> GetRequestedExistingServiceIds(IReadOnlyCollection<UpdateAdditionalServiceInput> availableServices)
        {
            return availableServices
                .Where(service => service.Id.HasValue)
                .Select(service => service.Id!.Value)
                .ToHashSet();
        }

        private static void ValidateServiceIds(Room room, HashSet<Guid> requestedExistingIds)
        {
            var existingServiceIds = room.AvailableServices
                .Select(service => service.Id)
                .ToHashSet();

            var invalidServiceIds = requestedExistingIds
                .Except(existingServiceIds)
                .ToList();

            if (invalidServiceIds.Count > 0)
            {
                throw new ArgumentException($"Service '{invalidServiceIds[0]}' does not belong to room '{room.Id}'.");
            }
        }

        private static List<AdditionalService> RemoveMissingServices(Room room, HashSet<Guid> requestedExistingIds)
        {
            var servicesToRemove = room.AvailableServices
                .Where(service => !requestedExistingIds.Contains(service.Id))
                .ToList();

            foreach (var service in servicesToRemove)
            {
                room.AvailableServices.Remove(service);
            }

            return servicesToRemove;
        }

        private static List<AdditionalService> UpdateAvailableServices(Room room, IReadOnlyCollection<UpdateAdditionalServiceInput> availableServices)
        {
            var servicesToAdd = new List<AdditionalService>();

            foreach (var input in availableServices)
            {
                if (input.Id.HasValue)
                {
                    var existingService = room.AvailableServices
                        .Single(service => service.Id == input.Id.Value);

                    existingService.Name = input.Name;
                    existingService.Price = input.Price;
                }
                else
                {
                    var newService = new AdditionalService
                    {
                        Id = Guid.NewGuid(),
                        RoomId = room.Id,
                        Name = input.Name,
                        Price = input.Price
                    };

                    room.AvailableServices.Add(newService);
                    servicesToAdd.Add(newService);
                }
            }

            return servicesToAdd;
        }

        private static void ValidateRoomProperties(string name, int capacity, decimal hourlyRate)
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

        private static void ValidateAvailableServices(IReadOnlyCollection<AdditionalServiceInput> availableServices)
        {
            foreach (var service in availableServices)
            {
                if (string.IsNullOrWhiteSpace(service.Name))
                {
                    throw new ArgumentException("Service name is required.", nameof(availableServices));
                }

                if (service.Price < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(availableServices), "Service price cannot be negative.");
                }
            }
        }

        private static void ValidateAvailableServices(IReadOnlyCollection<UpdateAdditionalServiceInput> availableServices)
        {
            foreach (var service in availableServices)
            {
                if (string.IsNullOrWhiteSpace(service.Name))
                {
                    throw new ArgumentException("Service name is required.", nameof(availableServices));
                }

                if (service.Price < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(availableServices), "Service price cannot be negative.");
                }
            }
        }

        private static void ValidateAvailabilitySearch(DateTime start, DateTime end, int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
            }

            if (start >= end)
            {
                throw new ArgumentException("Start time must be before end time.");
            }

            if (start.Minute != 0 || end.Minute != 0)
            {
                throw new ArgumentException("Start and end time must be on full hours.");
            }

            if (start.Date != end.Date || start.TimeOfDay < TimeSpan.FromHours(6) || end.TimeOfDay > TimeSpan.FromHours(23))
            {
                throw new ArgumentException("Time range must be within 06:00–23:00.");
            }
        }
    }
}
