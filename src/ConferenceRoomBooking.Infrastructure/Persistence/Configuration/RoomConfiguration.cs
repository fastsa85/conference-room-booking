using ConferenceRoomBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRoomBooking.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(room => room.Id);

        builder.Property(room => room.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(room => room.Capacity)
            .IsRequired();

        builder.Property(room => room.HourlyRate)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}