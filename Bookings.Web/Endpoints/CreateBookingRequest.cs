namespace Bookings.Web.Endpoints;

public record CreateBookingRequest(DateTimeOffset StartsAt, int DurationMinutes);