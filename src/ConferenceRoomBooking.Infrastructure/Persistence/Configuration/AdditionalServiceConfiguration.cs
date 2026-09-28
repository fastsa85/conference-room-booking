using ConferenceRoomBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRoomBooking.Infrastructure.Persistence.Configuration;

public class AdditionalServiceConfiguration : IEntityTypeConfiguration<AdditionalService>
{
    public void Configure(EntityTypeBuilder<AdditionalService> builder)
    {
        builder.HasKey(service => service.Id);

        builder.Property(service => service.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(service => service.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasOne(service => service.Room)
            .WithMany(room => room.AvailableServices)
            .HasForeignKey(service => service.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
