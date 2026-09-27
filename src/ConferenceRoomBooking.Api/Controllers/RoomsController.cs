using ConferenceRoomBooking.Api.Models.Rooms;
using ConferenceRoomBooking.Application.Rooms;
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

        return Ok(rooms);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var room = await _roomService.GetByIdAsync(id);

        if (room is null)
        {
            return NotFound();
        }

        return Ok(room);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomRequest request)
    {
        var room = await _roomService.CreateAsync(
            request.Name,
            request.Capacity,
            request.HourlyRate);

        return CreatedAtAction(
            nameof(GetById),
            new { id = room.Id },
            room
            );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateRoomRequest request)
    {
        var updated = await _roomService.UpdateAsync(
            id,
            request.Name,
            request.Capacity,
            request.HourlyRate);

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
}
