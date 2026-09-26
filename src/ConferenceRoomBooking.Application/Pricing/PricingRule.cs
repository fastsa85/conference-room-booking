namespace ConferenceRoomBooking.Application.Pricing;

public record PricingRule(
    TimeOnly From,
    TimeOnly To,
    decimal Multiplier
);

