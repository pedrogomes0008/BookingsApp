using Bookings.Core.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookings.Infrastructure.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Status)
               .HasConversion<string>()
               .HasMaxLength(20);
        builder.Property(b => b.Duration)
               .HasColumnName("DurationMinutes")
               .HasConversion(
                duration => (int)duration.TotalMinutes,    // C# → database
                minutes => TimeSpan.FromMinutes(minutes));  // database → C#
    }
}