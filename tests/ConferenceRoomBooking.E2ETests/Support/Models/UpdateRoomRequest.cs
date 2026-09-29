namespace ConferenceRoomBooking.E2ETests.Support.Models;

public class UpdateRoomRequest
{
    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public decimal HourlyRate { get; set; }

    public List<UpdateAdditionalServiceRequest> AvailableServices { get; set; } = [];
}

public class UpdateAdditionalServiceRequest
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
}