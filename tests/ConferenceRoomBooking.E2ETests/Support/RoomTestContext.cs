using ConferenceRoomBooking.E2ETests.Support.Models;

namespace ConferenceRoomBooking.E2ETests.Support;

public class RoomTestContext
{
    public HttpClient HttpClient { get; } = new()
    {
        BaseAddress = new Uri("http://localhost:8080")
    };

    public RoomRequest? RoomRequest { get; set; }

    public HttpResponseMessage? Response { get; set; }

    public RoomResponse? RoomResponse { get; set; }

    public Dictionary<string, Guid> RoomIds { get; } = [];

    public List<RoomResponse>? RoomsResponse { get; set; }

    public Guid? RoomId { get; set; }

    public BookingResponse? BookingResponse { get; set; }
}