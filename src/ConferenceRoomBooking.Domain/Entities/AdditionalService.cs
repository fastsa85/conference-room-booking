namespace ConferenceRoomBooking.Domain.Entities;

public class AdditionalService
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsActive { get; set; }
}