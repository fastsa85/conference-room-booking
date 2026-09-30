namespace ConferenceRoomBooking.Application.Rooms
{
    public record UpdateAdditionalServiceInput(
        Guid? Id,
        string Name,
        decimal Price);
}
