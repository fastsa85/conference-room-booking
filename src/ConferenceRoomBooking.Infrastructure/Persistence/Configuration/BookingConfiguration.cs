using ConferenceRoomBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRoomBooking.Infrastructure.Persistence.Configuration
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(booking => booking.Id);

            builder.Property(booking => booking.Status)
                .IsRequired();

            builder.Property(booking => booking.Start)
                .IsRequired();

            builder.Property(booking => booking.End)
                .IsRequired();

            builder.Property(booking => booking.TotalCost)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.HasOne(booking => booking.Room)
                .WithMany(room => room.Bookings)
                .HasForeignKey(booking => booking.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
