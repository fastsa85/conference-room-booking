using ConferenceRoomBooking.Domain.Enums;

namespace ConferenceRoomBooking.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }

    public BookingStatus Status { get; set; }

    public Guid RoomId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public decimal TotalCost { get; set; }

    public Room Room { get; set; } = null!;

    public ICollection<BookingAdditionalService> AdditionalServices { get; set; } = new List<BookingAdditionalService>();
}