using System;

namespace ConferenceRoomBooking.Application.Pricing;

internal record PricingSegment(
    DateTime Start,
    DateTime End,
    decimal Multiplier);
