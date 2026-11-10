namespace Bookings.Web.Endpoints;

public record RescheduleBookingRequest(DateTimeOffset NewStartsAt);