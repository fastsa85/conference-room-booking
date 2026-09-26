namespace ConferenceRoomBooking.Application.Bookings;

public class BookingValidator
{
    private readonly Func<DateTime> _now;

    public BookingValidator(Func<DateTime>? now = null)
    {
        _now = now ?? (() => DateTime.Now);
    }

    public IReadOnlyCollection<string> Validate(DateTime start, DateTime end)
    {
        var errors = new List<string>();

        if (start < _now())
        {
            errors.Add("Booking cannot start in the past.");
        }

        if (start >= end)
        {
            errors.Add("Booking start must be before booking end.");
        }

        if (start.Minute != 0 || end.Minute != 0)
        {
            errors.Add("Booking start and end must be on full hours.");
        }

        if (start.Date != end.Date  || start.TimeOfDay < TimeSpan.FromHours(6) || end.TimeOfDay > TimeSpan.FromHours(23))
        {
            errors.Add("Booking must be within 06:00–23:00.");
        }

        return errors;
    }
}
