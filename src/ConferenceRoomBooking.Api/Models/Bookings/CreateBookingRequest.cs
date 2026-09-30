namespace ConferenceRoomBooking.Api.Models.Bookings;

public class CreateBookingRequest
{
    public Guid RoomId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public List<Guid> AdditionalServiceIds { get; set; } = [];
}