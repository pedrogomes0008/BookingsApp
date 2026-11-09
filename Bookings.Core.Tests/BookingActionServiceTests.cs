using Bookings.Core.Application.Bookings;
using Bookings.Core.Domain.Bookings;
using Bookings.Core.Tests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace Bookings.Core.Tests;

public class BookingActionServiceTests
{
    private readonly DateTimeOffset _now = new(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
    private readonly FakeTimeProvider _timeProvider;
    private readonly InMemoryBookingRepository _repository = new();
    private readonly BookingActionService _service;

    public BookingActionServiceTests()
    {
        _timeProvider = new FakeTimeProvider(_now);
        _service = new BookingActionService(_repository, _timeProvider);
    }

    [Fact]
    public async Task ApproveAsync_WhenPending_SavesConfirmed()
    {
        // Arrange
        var booking = await CreatePendingBookingAsync();

        // Act
        await _service.ApproveAsync(booking.Id, CancellationToken.None);

        // Assert
        var saved = Assert.Single(_repository.SavedBookings);
        Assert.Equal(BookingStatus.Confirmed, saved.Status);
    }

    [Fact]
    public async Task ApproveAsync_WhenMissing_ThrowsNotFound()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(() => _service.ApproveAsync(guid, CancellationToken.None));
        Assert.Equal(guid, exception.BookingId);
    }

    [Fact]
    public async Task CancelAsync_BeforeStart_SavesCancelled()
    {
        // Arrange
        var booking = await CreateConfirmedBookingAsync();

        // Act
        await _service.CancelAsync(booking.Id, CancellationToken.None);

        // Assert
        var saved = Assert.Single(_repository.SavedBookings);
        Assert.Equal(BookingStatus.Cancelled, saved.Status);
    }

    [Fact]
    public async Task CancelAsync_AfterStart_Throws()
    {
        // Arrange
        var booking = await CreateConfirmedBookingAsync();
        _timeProvider.Advance(TimeSpan.FromHours(2));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelAsync(booking.Id, CancellationToken.None));
        Assert.Empty(_repository.SavedBookings);
    }

    [Fact]
    public async Task RescheduleAsync_SavesNewStartAndConfirmed()
    {
        // Arrange
        var booking = await CreatePendingBookingAsync();
        var newStartDate = booking.StartsAt.AddDays(1);

        // Act
        await _service.RescheduleAsync(booking.Id, newStartDate, CancellationToken.None);

        // Assert
        var saved = Assert.Single(_repository.SavedBookings);
        Assert.Equal(BookingStatus.Confirmed, saved.Status);
        Assert.Equal(newStartDate, saved.StartsAt);
    }

    private Booking CreatePendingBooking()
    {
        var startsAt = _now.AddHours(1);
        var duration = TimeSpan.FromMinutes(60);

        var booking = Booking.Request(startsAt, duration, _now, TimeSpan.FromHours(24));

        return booking;
    }

    private async Task<Booking> CreatePendingBookingAsync()
    {
        var booking = CreatePendingBooking();

        await _repository.AddAsync(booking, CancellationToken.None);

        return booking;
    }

    private async Task<Booking> CreateConfirmedBookingAsync()
    {
        var booking = CreatePendingBooking();

        booking.Approve();

        await _repository.AddAsync(booking, CancellationToken.None);

        return booking;
    }
}