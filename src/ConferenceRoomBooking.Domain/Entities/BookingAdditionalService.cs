namespace ConferenceRoomBooking.Domain.Entities;

public class BookingAdditionalService
{
    public Guid Id { get; set; }

    public Guid AdditionalServiceId { get; set; }

    public decimal Price { get; set; }

    public AdditionalService AdditionalService { get; set; } = null!;
}