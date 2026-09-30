namespace ConferenceRoomBooking.Domain.Entities;

public class BookingAdditionalService
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }
    public Guid AdditionalServiceId { get; set; }

    public decimal Price { get; set; }

    public Booking Booking { get; set; } = null!;
    public AdditionalService AdditionalService { get; set; } = null!;
}