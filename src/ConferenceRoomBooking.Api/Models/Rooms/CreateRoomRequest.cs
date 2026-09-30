using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomBooking.Api.Models.Rooms;

public class CreateRoomRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal HourlyRate { get; set; }

    public List<AdditionalServiceRequest> AvailableServices { get; set; } = [];
}
