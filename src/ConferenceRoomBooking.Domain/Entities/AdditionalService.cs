namespace ConferenceRoomBooking.Domain.Entities;

public class AdditionalService
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }

    public string Name { get; set; } = null!;
    public decimal Price { get; set; }

    public Room Room { get; set; } = null!;
}