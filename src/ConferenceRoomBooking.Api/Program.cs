using ConferenceRoomBooking.Application.Rooms;
using ConferenceRoomBooking.Infrastructure.Persistence;
using ConferenceRoomBooking.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IRoomService, RoomService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

// Expose Program for WebApplicationFactory<Program> in integration tests
public partial class Program;