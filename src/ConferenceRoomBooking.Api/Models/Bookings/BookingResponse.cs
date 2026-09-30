namespace ConferenceRoomBooking.Api.Models.Bookings;

public class BookingResponse
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public string Status { get; set; } = null!;

    public decimal TotalCost { get; set; }

    public List<BookingAdditionalServiceResponse> AdditionalServices { get; set; } = [];
}