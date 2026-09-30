namespace ConferenceRoomBooking.Api.Models.Rooms;

public class AdditionalServiceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
}
