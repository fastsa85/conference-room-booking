namespace ConferenceRoomBooking.E2ETests.Support.Models;

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

public class BookingAdditionalServiceResponse
{
    public Guid AdditionalServiceId { get; set; }

    public decimal Price { get; set; }
}