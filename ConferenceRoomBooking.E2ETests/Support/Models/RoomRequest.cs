namespace ConferenceRoomBooking.E2ETests.Support.Models;

public class RoomRequest
{
    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public decimal HourlyRate { get; set; }

    public List<AdditionalServiceRequest> AvailableServices { get; set; } = [];
}

public class AdditionalServiceRequest
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
}
