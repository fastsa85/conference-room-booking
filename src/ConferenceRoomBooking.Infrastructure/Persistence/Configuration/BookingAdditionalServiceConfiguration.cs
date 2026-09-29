using ConferenceRoomBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRoomBooking.Infrastructure.Persistence.Configuration
{
    public class BookingAdditionalServiceConfiguration : IEntityTypeConfiguration<BookingAdditionalService>
    {
        public void Configure(EntityTypeBuilder<BookingAdditionalService> builder)
        {
            builder.HasKey(service => service.Id);

            builder.Property(service => service.Price)
                .IsRequired()
                .HasPrecision(18, 2);
        }
    }
}
