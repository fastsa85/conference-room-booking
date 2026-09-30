using ConferenceRoomBooking.Api.Models.Bookings;
using ConferenceRoomBooking.Application.Bookings;
using ConferenceRoomBooking.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBooking.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingRequest request)
    {
        try
        {
            var booking = await _bookingService.CreateAsync(
                request.RoomId,
                request.Start,
                request.End,
                request.AdditionalServiceIds);

            return StatusCode(StatusCodes.Status201Created, ToResponse(booking));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    private static BookingResponse ToResponse(Booking booking)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            Start = booking.Start,
            End = booking.End,
            Status = booking.Status.ToString(),
            TotalCost = booking.TotalCost,

            AdditionalServices = booking.AdditionalServices.Select(service =>
                new BookingAdditionalServiceResponse
                {
                    AdditionalServiceId = service.AdditionalServiceId,
                    Price = service.Price
                }).ToList()
        };
    }
}