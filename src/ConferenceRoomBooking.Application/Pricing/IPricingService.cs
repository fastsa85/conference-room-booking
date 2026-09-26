namespace ConferenceRoomBooking.Application.Pricing;

public interface IPricingService
{
    decimal CalculateRoomCost(decimal hourlyRate, DateTime start, DateTime end);
}