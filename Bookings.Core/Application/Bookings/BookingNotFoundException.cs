namespace Bookings.Core.Application.Bookings;

public class BookingNotFoundException : Exception
{
    public Guid BookingId { get; }

    public BookingNotFoundException(Guid bookingId)
    : base($"Booking '{bookingId}' was not found.")
    {
        BookingId = bookingId;
    }
}