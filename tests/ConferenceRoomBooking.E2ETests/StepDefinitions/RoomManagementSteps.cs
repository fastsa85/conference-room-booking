using ConferenceRoomBooking.E2ETests.Support;
using ConferenceRoomBooking.E2ETests.Support.Models;
using NUnit.Framework;
using System.Net.Http.Json;

namespace ConferenceRoomBooking.E2ETests.StepDefinitions;

[Binding]
public class RoomManagementSteps
{
    private const string RoomsEndpoint = "/api/rooms";
    private const string AvailableRoomsEndpoint = "/api/rooms/available";

    private readonly RoomTestContext _context;

    public RoomManagementSteps(RoomTestContext context)
    {
        _context = context;
    }

    [Given("a room with the following details")]
    public void GivenARoomWithTheFollowingDetails(Table table)
    {
        var row = table.Rows.Single();

        _context.RoomRequest = new RoomRequest
        {
            Name = row["Name"],
            Capacity = int.Parse(row["Capacity"]),
            HourlyRate = decimal.Parse(row["HourlyRate"])
        };
    }

    [Given("the room has the following services")]
    public void GivenTheRoomHasTheFollowingServices(Table table)
    {
        Assert.That(_context.RoomRequest, Is.Not.Null);

        _context.RoomRequest!.AvailableServices = table.Rows
            .Select(row => new AdditionalServiceRequest
            {
                Name = row["Name"],
                Price = decimal.Parse(row["Price"])
            })
            .ToList();
    }

    [When("the client creates the room")]
    public async Task WhenTheClientCreatesTheRoom()
    {
        Assert.That(_context.RoomRequest, Is.Not.Null);

        _context.Response = await _context.HttpClient.PostAsJsonAsync(RoomsEndpoint, _context.RoomRequest);

        if (_context.Response.IsSuccessStatusCode)
        {
            _context.RoomResponse =
                await _context.Response.Content.ReadFromJsonAsync<RoomResponse>();

            _context.RoomId = _context.RoomResponse?.Id;

            if (_context.RoomResponse is not null)
            {
                _context.RoomIds[_context.RoomResponse.Name] = _context.RoomResponse.Id;
            }
        }
    }

    [When("the client requests all rooms")]
    public async Task WhenTheClientRequestsAllRooms()
    {
        _context.Response =
            await _context.HttpClient.GetAsync(RoomsEndpoint);

        if (_context.Response.IsSuccessStatusCode)
        {
            _context.RoomsResponse =
                await _context.Response.Content
                    .ReadFromJsonAsync<List<RoomResponse>>();
        }
    }

    [When("the client requests the room")]
    public async Task WhenTheClientRequestsTheRoom()
    {
        Assert.That(_context.RoomId, Is.Not.Null);

        _context.Response = await _context.HttpClient.GetAsync($"{RoomsEndpoint}/{_context.RoomId}");

        if (_context.Response.IsSuccessStatusCode)
        {
            _context.RoomResponse =
                await _context.Response.Content.ReadFromJsonAsync<RoomResponse>();
        }
    }

    [When("the client updates the room")]
    public async Task WhenTheClientUpdatesTheRoom()
    {
        Assert.That(_context.RoomId, Is.Not.Null);
        Assert.That(_context.RoomRequest, Is.Not.Null);
        Assert.That(_context.RoomResponse, Is.Not.Null);

        var updateRequest = new UpdateRoomRequest
        {
            Name = _context.RoomRequest!.Name,
            Capacity = _context.RoomRequest.Capacity,
            HourlyRate = _context.RoomRequest.HourlyRate,

            AvailableServices = _context.RoomRequest.AvailableServices
                .Select(service => new UpdateAdditionalServiceRequest
                {
                    Id = _context.RoomResponse!.AvailableServices
                        .SingleOrDefault(existing =>
                            existing.Name == service.Name)
                        ?.Id,

                    Name = service.Name,
                    Price = service.Price
                })
                .ToList()
        };

        _context.Response = await _context.HttpClient.PutAsJsonAsync($"{RoomsEndpoint}/{_context.RoomId}", updateRequest);
    }

    [When("the client deletes the room")]
    public async Task WhenTheClientDeletesTheRoom()
    {
        Assert.That(_context.RoomId, Is.Not.Null);

        _context.Response = await _context.HttpClient.DeleteAsync($"{RoomsEndpoint}/{_context.RoomId}");
    }

    [When(@"the client requests the room ""(.*)""")]
    public async Task WhenTheClientRequestsTheRoom(string roomName)
    {
        Assert.That(_context.RoomIds.ContainsKey(roomName), Is.True);

        var roomId = _context.RoomIds[roomName];

        _context.Response = await _context.HttpClient.GetAsync($"{RoomsEndpoint}/{roomId}");

        if (_context.Response.IsSuccessStatusCode)
        {
            _context.RoomResponse = await _context.Response.Content.ReadFromJsonAsync<RoomResponse>();
        }
    }

    [When("the client searches for available rooms")]
    public async Task WhenTheClientSearchesForAvailableRooms(Table table)
    {
        var row = table.Rows.Single();

        var start = DateTime.Parse(row["Start"]);
        var end = DateTime.Parse(row["End"]);
        var capacity = int.Parse(row["Capacity"]);

        var url = $"{AvailableRoomsEndpoint}?start={start:yyyy-MM-ddTHH:mm:ss}&end={end:yyyy-MM-ddTHH:mm:ss}&capacity={capacity}";

        _context.Response = await _context.HttpClient.GetAsync(url);

        if (_context.Response.IsSuccessStatusCode)
        {
            _context.RoomsResponse = await _context.Response.Content
                .ReadFromJsonAsync<List<RoomResponse>>();
        }
    }

    [Then("the response status code should be {int}")]
    public void ThenTheResponseStatusCodeShouldBe(int expectedStatusCode)
    {
        Assert.That(_context.Response, Is.Not.Null);

        Assert.That((int)_context.Response!.StatusCode, Is.EqualTo(expectedStatusCode));
    }

    [Then("the room should have the following details")]
    public void ThenTheRoomShouldHaveTheFollowingDetails(Table table)
    {
        Assert.That(_context.RoomResponse, Is.Not.Null);

        var expected = table.Rows.Single();

        Assert.Multiple(() =>
        {
            Assert.That(
                _context.RoomResponse!.Name,
                Is.EqualTo(expected["Name"]));

            Assert.That(
                _context.RoomResponse.Capacity,
                Is.EqualTo(int.Parse(expected["Capacity"])));

            Assert.That(
                _context.RoomResponse.HourlyRate,
                Is.EqualTo(decimal.Parse(expected["HourlyRate"])));
        });
    }

    [Then("the room should have the following services")]
    public void ThenTheRoomShouldHaveTheFollowingServices(Table table)
    {
        Assert.That(_context.RoomResponse, Is.Not.Null);

        Assert.That(
            _context.RoomResponse!.AvailableServices,
            Has.Count.EqualTo(table.Rows.Count));

        foreach (var expected in table.Rows)
        {
            var actual = _context.RoomResponse.AvailableServices
                .SingleOrDefault(service =>
                    service.Name == expected["Name"]);

            Assert.That(
                actual,
                Is.Not.Null,
                $"Service '{expected["Name"]}' was not found.");

            Assert.That(
                actual!.Price,
                Is.EqualTo(decimal.Parse(expected["Price"])));
        }
    }

    [Then("the following rooms should be returned")]
    public void ThenTheFollowingRoomsShouldBeReturned(Table table)
    {
        Assert.That(_context.RoomsResponse, Is.Not.Null);

        var expectedRoomNames = table.Rows
            .Select(row => row["Name"])
            .ToList();

        var actualRoomNames = _context.RoomsResponse!
            .Select(room => room.Name)
            .ToList();

        Assert.That(
            actualRoomNames,
            Is.EquivalentTo(expectedRoomNames));
    }

    [Then("the room {string} should not be returned")]
    public void ThenTheRoomShouldNotBeReturned(string roomName)
    {
        Assert.That(
            _context.RoomsResponse,
            Is.Not.Null);

        Assert.That(
            _context.RoomsResponse!.Any(room => room.Name == roomName),
            Is.False);
    }
}