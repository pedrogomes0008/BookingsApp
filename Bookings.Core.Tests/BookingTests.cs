using Bookings.Core.Domain.Bookings;

namespace Bookings.Core.Tests;

public class BookingTests
{
    [Fact]
    public void Request_CreateBooking_Success()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var startsAt = now.AddDays(1);
        var duration = TimeSpan.FromMinutes(60);

        // Act
        var booking = Booking.Request(startsAt, duration, now, TimeSpan.FromHours(24));

        // Assert
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Equal(now, booking.CreatedAt);
        Assert.Equal(startsAt, booking.StartsAt);
        Assert.Equal(duration, booking.Duration);
        Assert.NotEqual(Guid.Empty, booking.Id);
    }

    [Fact]
    public void Request_BookInThePast_Throws()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var startsAt = now.AddDays(-1);
        var duration = TimeSpan.FromMinutes(60);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Booking.Request(startsAt, duration, now, TimeSpan.FromHours(24)));
    }

    [Fact]
    public void Request_BookNow_Throws()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var startsAt = now;
        var duration = TimeSpan.FromMinutes(60);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Booking.Request(startsAt, duration, now, TimeSpan.FromHours(24)));
    }

    [Fact]
    public void Request_WithZeroDuration_Throws()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var startsAt = now.AddDays(1);
        var duration = TimeSpan.Zero;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Booking.Request(startsAt, duration, now, TimeSpan.FromHours(24)));
    }

    [Fact]
    public void Approve_WhenPending_SetsConfirmed()
    {
        // Arrange
        var booking = CreatePendingBooking();

        // Act
        booking.Approve();

        // Assert
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public void Approve_WhenAlreadyConfirmed_Throws()
    {
        // Arrange
        var booking = CreatePendingBooking();
        booking.Approve();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.Approve());
    }

    [Fact]
    public void Reject_WhenPending_SetsRejected()
    {
        // Arrange
        var booking = CreatePendingBooking();

        // Act
        booking.Reject();

        // Assert
        Assert.Equal(BookingStatus.Rejected, booking.Status);
    }

    [Fact]
    public void Reject_WhenConfirmed_Throws()
    {
        // Arrange
        var booking = CreatePendingBooking();
        booking.Approve();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.Reject());
    }

    [Fact]
    public void Approve_WhenRejected_Throws()
    {
        // Arrange
        var booking = CreatePendingBooking();
        booking.Reject();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.Approve());
    }

    [Fact]
    public void Cancel_BeforeStart_SetsCancelled()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.StartsAt.AddMinutes(-30);

        // Act
        booking.Cancel(now);

        // Assert
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public void Cancel_WhenStarted_Throws()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.StartsAt.AddMinutes(30);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.Cancel(now));
    }

    [Fact]
    public void Cancel_WhenPending_Throws()
    {
        // Arrange
        var booking = CreatePendingBooking();
        var now = booking.StartsAt.AddMinutes(-30);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.Cancel(now));
    }

    [Fact]
    public void Cancel_AtStart_Throws()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.StartsAt;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.Cancel(now));
    }

    [Fact]
    public void MarkNoShow_AfterStart_SetsNoShow()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.StartsAt.AddMinutes(30);

        // Act
        booking.MarkNoShow(now);

        // Assert
        Assert.Equal(BookingStatus.NoShow, booking.Status);
    }

    [Fact]
    public void MarkNoShow_WhenNotStarted_Throws()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.StartsAt.AddMinutes(-30);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.MarkNoShow(now));
    }

    [Fact]
    public void MarkNoShow_WhenPending_Throws()
    {
        // Arrange
        var booking = CreatePendingBooking();
        var now = booking.StartsAt.AddMinutes(30);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.MarkNoShow(now));
    }

    [Fact]
    public void MarkNoShow_AtStart_SetsNoShow()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.StartsAt;

        // Act
        booking.MarkNoShow(now);

        // Assert
        Assert.Equal(BookingStatus.NoShow, booking.Status);
    }

    [Fact]
    public void Request_StartFarAway_ExpiresAfterMaxWindow()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var startsAt = now.AddDays(3);
        var duration = TimeSpan.FromMinutes(60);

        // Act
        var booking = Booking.Request(startsAt, duration, now, TimeSpan.FromHours(24));

        // Assert
        Assert.Equal(now.AddHours(24), booking.ExpiresAt);
    }

    [Fact]
    public void Request_StartSoon_ExpiresAtHalfTimeUntilStart()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var startsAt = now.AddHours(10);
        var duration = TimeSpan.FromMinutes(60);

        // Act
        var booking = Booking.Request(startsAt, duration, now, TimeSpan.FromHours(24));

        // Assert
        Assert.Equal(now.AddHours(5), booking.ExpiresAt);
    }

    [Fact]
    public void Request_WithZeroApprovalWindow_Throws()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var startsAt = now.AddDays(1);
        var duration = TimeSpan.FromMinutes(60);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Booking.Request(startsAt, duration, now, TimeSpan.Zero));
    }

    [Fact]
    public void Request_WithPartialMinutes_Throws()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var startsAt = now.AddDays(1);
        var duration = TimeSpan.FromMinutes(60) + TimeSpan.FromSeconds(30);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Booking.Request(startsAt, duration, now, TimeSpan.FromHours(24)));
    }

    [Fact]
    public void Expire_AfterDeadline_SetsExpired()
    {
        // Arrange
        var booking = CreatePendingBooking();
        var now = booking.ExpiresAt.AddMinutes(10);

        // Act 
        booking.Expire(now);

        // Assert
        Assert.Equal(BookingStatus.Expired, booking.Status);
    }

    [Fact]
    public void Expire_AtDeadline_SetsExpired()
    {
        // Arrange
        var booking = CreatePendingBooking();
        var now = booking.ExpiresAt;

        // Act 
        booking.Expire(now);

        // Assert
        Assert.Equal(BookingStatus.Expired, booking.Status);
    }

    [Fact]
    public void Expire_BeforeDeadline_Throws()
    {
        // Arrange
        var booking = CreatePendingBooking();
        var now = booking.ExpiresAt.AddMinutes(-10);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.Expire(now));
    }

    [Fact]
    public void Expire_WhenConfirmed_Throws()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.ExpiresAt.AddMinutes(10);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.Expire(now));
    }

    [Fact]
    public void Reschedule_WhenPending_SetsConfirmedAndNewStart()
    {
        // Arrange
        var booking = CreatePendingBooking();
        var now = booking.StartsAt.AddHours(-1);
        var newStart = booking.StartsAt.AddDays(2);

        // Act
        booking.Reschedule(newStart, now);

        // Assert
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Equal(newStart, booking.StartsAt);
    }

    [Fact]
    public void Reschedule_WhenExpired_SetsConfirmed()
    {
        // Arrange
        var booking = CreateExpiredBooking();
        var now = booking.StartsAt.AddHours(-1);
        var newStart = booking.StartsAt.AddDays(2);

        // Act
        booking.Reschedule(newStart, now);

        // Assert
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Equal(newStart, booking.StartsAt);
    }

    [Fact]
    public void Reschedule_WhenConfirmedBeforeStart_UpdatesStart()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.StartsAt.AddHours(-1);
        var newStart = booking.StartsAt.AddDays(2);

        // Act
        booking.Reschedule(newStart, now);

        // Assert
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Equal(newStart, booking.StartsAt);
    }

    [Fact]
    public void Reschedule_WhenConfirmedAfterStart_Throws()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.StartsAt.AddHours(2);
        var newStart = booking.StartsAt.AddDays(2);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.Reschedule(newStart, now));
    }

    [Fact]
    public void Reschedule_WhenConfirmedAtStart_Throws()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.StartsAt;
        var newStart = booking.StartsAt.AddDays(2);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.Reschedule(newStart, now));
    }

    [Fact]
    public void Reschedule_WhenRejected_Throws()
    {
        // Arrange
        var booking = CreateRejectedBooking();
        var now = booking.StartsAt.AddHours(-1);
        var newStart = booking.StartsAt.AddDays(2);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.Reschedule(newStart, now));
    }

    [Fact]
    public void Reschedule_WhenCancelled_Throws()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.StartsAt.AddHours(-1);
        var newStart = booking.StartsAt.AddDays(2);
        booking.Cancel(now);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => booking.Reschedule(newStart, now));
    }

    [Fact]
    public void Reschedule_ToPastDate_Throws()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.StartsAt.AddHours(-1);
        var newStart = now.AddDays(-2);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => booking.Reschedule(newStart, now));
    }

    [Fact]
    public void Reschedule_ToNow_Throws()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.StartsAt.AddHours(-1);
        var newStart = now;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => booking.Reschedule(newStart, now));
    }

    [Fact]
    public void Reschedule_ToSameStart_Throws()
    {
        // Arrange
        var booking = CreateConfirmedBooking();
        var now = booking.StartsAt.AddHours(-1);
        var newStart = booking.StartsAt;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => booking.Reschedule(newStart, now));
    }


    private static Booking CreatePendingBooking()
    {
        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var startsAt = now.AddDays(1);
        var duration = TimeSpan.FromMinutes(60);

        return Booking.Request(startsAt, duration, now, TimeSpan.FromHours(24));
    }

    private static Booking CreateConfirmedBooking()
    {
        var booking = CreatePendingBooking();
        booking.Approve();

        return booking;
    }

    private static Booking CreateRejectedBooking()
    {
        var booking = CreatePendingBooking();
        booking.Reject();

        return booking;
    }

    private static Booking CreateExpiredBooking()
    {
        var booking = CreatePendingBooking();
        booking.Expire(booking.ExpiresAt);

        return booking;
    }
}
