using ConferenceRoomBooking.E2ETests.Support;
using ConferenceRoomBooking.E2ETests.Support.Models;
using NUnit.Framework;
using System.Net.Http.Json;

namespace ConferenceRoomBooking.E2ETests.StepDefinitions;

[Binding]
public class RoomBookingSteps
{
    private const string BookingsEndpoint = "/api/bookings";
    private readonly RoomTestContext _context;

    public RoomBookingSteps(RoomTestContext context)
    {
        _context = context;
    }

    [When("the client books the room")]
    public async Task WhenTheClientBooksTheRoom(Table table)
    {
        var row = table.Rows.Single();

        var start = DateTime.Parse(row["Start"]);
        var end = DateTime.Parse(row["End"]);

        var serviceNames = row["Services"].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        Assert.That(_context.RoomResponse,Is.Not.Null,
            "Room must be created before it can be booked.");

        var room = _context.RoomResponse!;

        var serviceIds = serviceNames.Select(serviceName =>
            {
                var service = room.AvailableServices.SingleOrDefault(
                    service => service.Name == serviceName);

                Assert.That(
                    service,
                    Is.Not.Null,
                    $"Service '{serviceName}' was not found in the created room.");

                return service!.Id;
            }).ToList();

        var request = new
        {
            RoomId = room.Id,
            Start = start,
            End = end,
            AdditionalServiceIds = serviceIds
        };

        _context.Response = await _context.HttpClient.PostAsJsonAsync(BookingsEndpoint, request);

        if (_context.Response.IsSuccessStatusCode)
        {
            _context.BookingResponse = await _context.Response.Content.ReadFromJsonAsync<BookingResponse>();
        }
    }

    [Then("the booking should have total cost {decimal}")]
    public void ThenTheBookingShouldHaveTotalCost(decimal expectedTotalCost)
    {
        Assert.That(_context.BookingResponse, Is.Not.Null);

        Assert.That(_context.BookingResponse!.TotalCost, Is.EqualTo(expectedTotalCost));
    }

    [Then("the booking status should be {string}")]
    public void ThenTheBookingStatusShouldBe(string expectedStatus)
    {
        Assert.That(_context.BookingResponse, Is.Not.Null);

        Assert.That(_context.BookingResponse!.Status, Is.EqualTo(expectedStatus));
    }
}