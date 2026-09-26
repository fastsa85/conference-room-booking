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

        var hourlyRate = 2000m;
        var start = new DateTime(2026, 9, 26, 10, 0, 0);
        var end = new DateTime(2026, 9, 26, 12, 0, 0);

        // Act
        var result = pricingService.CalculateRoomCost(hourlyRate, start, end);

        // Assert
        Assert.That(result, Is.EqualTo(4000m));
    }

    [Test]
    public void CalculateRoomCost_WhenBookingIsWithinMorningDiscountPeriod_Applies10PercentDiscount()
    {
        // Arrange
        var pricingService = new PricingService();

        var hourlyRate = 2000m;
        var start = new DateTime(2026, 9, 26, 6, 0, 0);
        var end = new DateTime(2026, 9, 26, 8, 0, 0);

        // Act
        var result = pricingService.CalculateRoomCost(hourlyRate, start, end);

        // Assert
        Assert.That(result, Is.EqualTo(3600m));
    }

    [Test]
    public void CalculateRoomCost_WhenBookingCrossesPricingPeriods_AppliesEachPeriod()
    {
        // Arrange
        var pricingService = new PricingService();

        var hourlyRate = 2000m;

        // The booking crosses two pricing periods
        var start = new DateTime(2026, 9, 26, 8, 0, 0);
        var end = new DateTime(2026, 9, 26, 10, 0, 0);

        // Act
        var result = pricingService.CalculateRoomCost(hourlyRate, start, end);

        // Assert
        Assert.That(result, Is.EqualTo(3800m));
    }
}
