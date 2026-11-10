namespace Bookings.Core.Domain.Bookings;

public class BookingStateException : Exception
{
    public BookingStateException(string message)
    : base(message)
    {

    }
}