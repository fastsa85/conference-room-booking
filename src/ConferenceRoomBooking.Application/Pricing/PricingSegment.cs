namespace ConferenceRoomBooking.Application.Pricing;

public record PricingSegment(
    DateTime Start,
    DateTime End,
    decimal Multiplier);
