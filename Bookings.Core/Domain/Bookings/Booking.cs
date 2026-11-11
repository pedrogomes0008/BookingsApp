namespace Bookings.Core.Domain.Bookings;

public class Booking
{
    public Guid Id { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public TimeSpan Duration { get; private set; }

    public BookingStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private Booking()
    {

    }

    public static Booking Request(DateTimeOffset startsAt, TimeSpan duration, DateTimeOffset now, TimeSpan maxApprovalWindow)
    {
        if (startsAt <= now)
        {
            throw new ArgumentException("Starting date must be in the future");
        }

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentException("Duration of the booking must be greater than zero");
        }

        if ((duration.Ticks % TimeSpan.TicksPerMinute) != 0)
        {
            throw new ArgumentException("Duration must be a whole number of minutes");
        }

        if (maxApprovalWindow <= TimeSpan.Zero)
        {
            throw new ArgumentException("Max approval window for a booking must be greater than zero");
        }

        var halfTimeUntilStart = (startsAt - now) / 2;

        return new Booking
        {
            Id = Guid.NewGuid(),
            Status = BookingStatus.Pending,
            CreatedAt = now,
            StartsAt = startsAt,
            ExpiresAt = maxApprovalWindow >= halfTimeUntilStart ? now + halfTimeUntilStart : now + maxApprovalWindow,
            Duration = duration
        };
    }

    public void Approve()
    {
        if (Status != BookingStatus.Pending)
        {
            throw new BookingStateException("Only pending bookings can be approved.");
        }

        Status = BookingStatus.Confirmed;
    }

    public void Reject()
    {
        if (Status != BookingStatus.Pending)
        {
            throw new BookingStateException("Only pending bookings can be rejected.");
        }

        Status = BookingStatus.Rejected;
    }

    public void Cancel(DateTimeOffset now)
    {
        if (Status != BookingStatus.Confirmed)
        {
            throw new BookingStateException("Only confirmed bookings can be cancelled.");
        }

        if (now >= StartsAt)
        {
            throw new BookingStateException("Bookings that already started cannot be cancelled.");
        }

        Status = BookingStatus.Cancelled;
    }

    public void MarkNoShow(DateTimeOffset now)
    {
        if (Status != BookingStatus.Confirmed)
        {
            throw new BookingStateException("Only confirmed bookings can be marked as no-show.");
        }

        if (now < StartsAt)
        {
            throw new BookingStateException("Bookings that have not started cannot be marked as no-show.");
        }

        Status = BookingStatus.NoShow;
    }

    public void Expire(DateTimeOffset now)
    {
        if (Status != BookingStatus.Pending)
        {
            throw new BookingStateException("Only pending bookings can be expired.");
        }

        if (now < ExpiresAt)
        {
            throw new BookingStateException("Bookings cannot be expired before their approval deadline.");
        }

        Status = BookingStatus.Expired;
    }

    public void Reschedule(DateTimeOffset newStartsAt, DateTimeOffset now)
    {
        if (newStartsAt <= now)
        {
            throw new ArgumentException("Starting date must be in the future.");
        }

        if (newStartsAt == StartsAt)
        {
            throw new ArgumentException("New starting date must be different from the previous one.");
        }

        if (Status is not (BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.Expired))
        {
            throw new BookingStateException("Only pending, expired or confirmed bookings can be rescheduled.");
        }

        if (Status is BookingStatus.Confirmed && now >= StartsAt)
        {
            throw new BookingStateException("Bookings that already started cannot be rescheduled.");
        }

        StartsAt = newStartsAt;
        Status = BookingStatus.Confirmed;
    }
}