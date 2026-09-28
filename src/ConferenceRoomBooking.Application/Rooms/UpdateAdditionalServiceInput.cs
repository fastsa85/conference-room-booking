using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceRoomBooking.Application.Rooms
{
    public record UpdateAdditionalServiceInput(
        Guid? Id,
        string Name,
        decimal Price);
}
