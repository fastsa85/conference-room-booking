using ConferenceRoomBooking.Api.Models.Rooms;
using ConferenceRoomBooking.Application.Rooms;
using ConferenceRoomBooking.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBooking.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _roomService.GetAllAsync();

        return Ok(rooms.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var room = await _roomService.GetByIdAsync(id);

        if (room is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(room));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomRequest request)  
    {
        var availableServices = request.AvailableServices
            .Select(service => new AdditionalServiceInput(
                    service.Name,
                    service.Price))
            .ToList();
            
        var room = await _roomService.CreateAsync(
            request.Name,
            request.Capacity,
            request.HourlyRate,
            availableServices);

        return CreatedAtAction(
            nameof(GetById),
            new { id = room.Id },
            ToResponse(room)
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateRoomRequest request)
    {
        var availableServices = request.AvailableServices
        .Select(service => new UpdateAdditionalServiceInput(
            service.Id,
            service.Name,
            service.Price))
        .ToList();

        var updated = await _roomService.UpdateAsync(
            id,
            request.Name,
            request.Capacity,
            request.HourlyRate,
            availableServices);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _roomService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static RoomResponse ToResponse(Room room)
    {
        return new RoomResponse
        {
            Id = room.Id,
            Name = room.Name,
            Capacity = room.Capacity,
            HourlyRate = room.HourlyRate,
            AvailableServices = room.AvailableServices
                .Select(service => new AdditionalServiceResponse
                {
                    Id = service.Id,
                    Name = service.Name,
                    Price = service.Price
                })
                .ToList()
        };
    }
}
