namespace ConferenceRoomBooking.E2ETests.Support.Models;

public class RoomResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public decimal HourlyRate { get; set; }

    public List<AdditionalServiceResponse> AvailableServices { get; set; } = [];
}

public class AdditionalServiceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
}
