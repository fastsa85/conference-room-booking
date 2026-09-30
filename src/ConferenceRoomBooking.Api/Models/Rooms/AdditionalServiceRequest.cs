using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomBooking.Api.Models.Rooms;

public class AdditionalServiceRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}
