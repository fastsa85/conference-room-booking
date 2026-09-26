using System;
using ConferenceRoomBooking.Application.Pricing;

namespace ConferenceRoomBooking.UnitTests.Application.Pricing;

public class PricingServiceTests
{
    [Test]
    public void CalculateRoomCost_WhenBookingIsWithinStandardPeriod_ReturnsStandardCost()
    {
        // Arrange
        var pricingService = new PricingService();

        var hourlyRate = 1000m;
        // The booking is  within the standard period:
        // 10:00–12:00 => 1000 × 2 hours = 2000
        var start = new DateTime(2026, 9, 26, 10, 0, 0);
        var end = new DateTime(2026, 9, 26, 12, 0, 0);

        // Act
        var result = pricingService.CalculateRoomCost(hourlyRate, start, end);

        // Assert
        Assert.That(result, Is.EqualTo(2000m));
    }

    [Test]
    public void CalculateRoomCost_WhenBookingIsWithinMorningDiscountPeriod_Applies10PercentDiscount()
    {
        // Arrange
        var pricingService = new PricingService();

        var hourlyRate = 1000m;
        // The booking is  within the morning discount period:
        // 06:00–08:00 => 10% discount: 1000 × 2 hours x 0.9 = 1800
        var start = new DateTime(2026, 9, 26, 6, 0, 0);
        var end = new DateTime(2026, 9, 26, 8, 0, 0);

        // Act
        var result = pricingService.CalculateRoomCost(hourlyRate, start, end);

        // Assert
        Assert.That(result, Is.EqualTo(1800m));
    }

    [Test]
    public void CalculateRoomCost_WhenBookingIsWithinEveningDiscountPeriod_Applies20PercentDiscount()
    {
        // Arrange
        var pricingService = new PricingService();

        var hourlyRate = 1000m;
        // The booking is within the evening discount period:
        // 18:00–22:00 => 20% discount: 1000 × 4 hours x 0.8 = 3200
        var start = new DateTime(2026, 9, 26, 18, 0, 0);
        var end = new DateTime(2026, 9, 26, 22, 0, 0);

        // Act
        var result = pricingService.CalculateRoomCost(hourlyRate, start, end);

        // Assert
        Assert.That(result, Is.EqualTo(3200m));
    }

    [Test]
    public void CalculateRoomCost_WhenBookingIsWithinPeakPeriod_Applies15PercentSurcharge()
    {
        // Arrange
        var pricingService = new PricingService();

        var hourlyRate = 1000m;
        var start = new DateTime(2026, 9, 26, 12, 0, 0);
        var end = new DateTime(2026, 9, 26, 14, 0, 0);

        // Act
        var result = pricingService.CalculateRoomCost(hourlyRate, start, end);

        // Assert
        Assert.That(result, Is.EqualTo(2300m));
    }

    [Test]
    public void CalculateRoomCost_WhenBookingCrossesPricingPeriods_AppliesEachPeriod()
    {
        // Arrange
        var pricingService = new PricingService();

        var hourlyRate = 1000m;

        // The booking crosses two pricing periods:
        // 08:00–09:00 => 10% discount: 900
        // 09:00–10:00 => standard rate: 1000
        // Total expected cost: 1900
        var start = new DateTime(2026, 9, 26, 8, 0, 0);
        var end = new DateTime(2026, 9, 26, 10, 0, 0);

        // Act
        var result = pricingService.CalculateRoomCost(hourlyRate, start, end);

        // Assert
        Assert.That(result, Is.EqualTo(1900m));
    }

    [Test]
    public void CalculateRoomCost_WhenBookingCrossesPeakPeriod_AppliesPeakRateOnlyDuringPeakPeriod()
    {
        // Arrange
        var pricingService = new PricingService();

        var hourlyRate = 1000m;

        // The booking crosses the peak period:
        // 11:00–12:00 => standard rate: 1000
        // 12:00–14:00 => 15% surcharge: 1000 x 2 hours x 1.15 = 2300
        // 14:00–15:00 => standard rate: 1000
        // Total expected cost: 4300
        var start = new DateTime(2026, 9, 26, 11, 0, 0);
        var end = new DateTime(2026, 9, 26, 15, 0, 0);

        // Act
        var result = pricingService.CalculateRoomCost(hourlyRate, start, end);

        // Assert
        Assert.That(result, Is.EqualTo(4300m));
    }

    [Test]
    public void CalculateRoomCost_WhenBookingStartsAtStandardPeriodBoundary_AppliesStandardRate()
    {
        // Arrange
        var pricingService = new PricingService();

        var hourlyRate = 1000m;

        // 09:00 is the boundary between the morning discount and standard period.
        // 09:00–10:00 → standard rate: 1000 × 1 hour = 1000
        var start = new DateTime(2026, 9, 26, 9, 0, 0);
        var end = new DateTime(2026, 9, 26, 10, 0, 0);

        // Act
        var result = pricingService.CalculateRoomCost(hourlyRate, start, end);

        // Assert
        Assert.That(result, Is.EqualTo(1000m));
    }

    [Test]
    public void CalculateRoomCost_WhenBookingStartsBeforePricingHours_ThrowsException()
    {
        // Arrange
        var pricingService = new PricingService();

        var hourlyRate = 1000m;

        // Pricing is only defined from 06:00 to 23:00.
        // 05:00–07:00 starts outside the supported pricing hours.
        var start = new DateTime(2026, 9, 26, 5, 0, 0);
        var end = new DateTime(2026, 9, 26, 7, 0, 0);

        // Act & Assert
        Assert.That(
            () => pricingService.CalculateRoomCost(hourlyRate, start, end),
            Throws.Exception);
    }

    [Test]
    public void CalculateRoomCost_WhenBookingStartsAtEveningPeriodBoundary_AppliesEveningDiscount()
    {
        // Arrange
        var pricingService = new PricingService();

        var hourlyRate = 1000m;

        // 18:00 is the boundary between the standard and evening discount periods.
        // 18:00–19:00 => 20% discount: 1000 × 1 hour × 0.8 = 800
        var start = new DateTime(2026, 9, 26, 18, 0, 0);
        var end = new DateTime(2026, 9, 26, 19, 0, 0);

        // Act
        var result = pricingService.CalculateRoomCost(hourlyRate, start, end);

        // Assert
        Assert.That(result, Is.EqualTo(800m));
    }
}
