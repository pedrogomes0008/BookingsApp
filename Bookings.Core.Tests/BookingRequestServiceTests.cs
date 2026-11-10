using Bookings.Core.Application.Bookings;
using Bookings.Core.Domain.Bookings;
using Bookings.Core.Tests.Fakes;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Bookings.Core.Tests;

public class BookingRequestServiceTests
{
    [Fact]
    public async Task RequestAsync_CreateBooking_Success()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(now);
        var bookingRepository = new InMemoryBookingRepository();
        var options = Options.Create(new BookingSettings { MaxApprovalWindowHours = 24 });
        var service = new BookingRequestService(timeProvider, bookingRepository, options);

        var startsAt = now.AddDays(1);
        var duration = TimeSpan.FromMinutes(60);

        // Act
        var booking = await service.RequestAsync(startsAt, duration, CancellationToken.None);

        // Assert
        Assert.Equal(now, booking.CreatedAt);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public async Task RequestAsync_AfterTimeAdvances_Throws()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(now);
        var bookingRepository = new InMemoryBookingRepository();
        var options = Options.Create(new BookingSettings { MaxApprovalWindowHours = 24 });
        var service = new BookingRequestService(timeProvider, bookingRepository, options);
        var startsAt = now.AddHours(1);
        var duration = TimeSpan.FromMinutes(60);

        // Act
        await service.RequestAsync(startsAt, duration, CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromHours(2));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.RequestAsync(startsAt, duration, CancellationToken.None));
    }

    [Fact]
    public async Task RequestAsync_SavesBookingInRepository()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(now);
        var bookingRepository = new InMemoryBookingRepository();
        var options = Options.Create(new BookingSettings { MaxApprovalWindowHours = 24 });
        var service = new BookingRequestService(timeProvider, bookingRepository, options);
        var startsAt = now.AddDays(1);
        var duration = TimeSpan.FromMinutes(60);

        // Act
        var booking = await service.RequestAsync(startsAt, duration, CancellationToken.None);

        // Assert
        var result = await bookingRepository.GetByIdAsync(booking.Id, CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
    }
}