namespace ConferenceRoomBooking.Application.Pricing;

public class PricingService : IPricingService
{
    private static readonly PricingRule[] Rules =
    [
        new(new TimeOnly(6, 0), new TimeOnly(9, 0), 0.90m),
        new(new TimeOnly(9, 0), new TimeOnly(12, 0), 1.00m),
        new(new TimeOnly(12, 0), new TimeOnly(14, 0), 1.15m),
        new(new TimeOnly(14, 0), new TimeOnly(18, 0), 1.00m),
        new(new TimeOnly(18, 0), new TimeOnly(23, 0), 0.80m)
    ];

    public decimal CalculateRoomCost(decimal hourlyRate, DateTime start, DateTime end)
    {
        return BuildPricingSegments(start, end).Sum(segment =>
        {
            var duration = segment.End - segment.Start;
            return hourlyRate * (decimal)duration.TotalHours * segment.Multiplier;
        });
    }

    private IEnumerable<PricingSegment> BuildPricingSegments(DateTime start, DateTime end)
    {
        var current = start;

        while (current < end)
        {
            var currentTime = TimeOnly.FromDateTime(current);

            var rule = Rules.Single(rule => currentTime >= rule.From && currentTime < rule.To); // treating pricing periods as half-open intervals [From, To), so the periods don't overlap and xx:00 is included in the next period
            var ruleEnd = current.Date + rule.To.ToTimeSpan();

            var segmentEnd = ruleEnd < end ? ruleEnd : end;

            yield return new PricingSegment(current, segmentEnd, rule.Multiplier);

            current = segmentEnd;
        }
    }
}
