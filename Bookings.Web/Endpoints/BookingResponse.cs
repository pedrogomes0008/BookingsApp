using Bookings.Core.Application.Bookings;
using Bookings.Core.Domain.Bookings;

namespace Bookings.Web.Endpoints;

public record BookingResponse(Guid Id, DateTimeOffset StartsAt, int DurationMinutes, BookingStatus Status, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt)
{
    public static BookingResponse From(Booking booking) => new(booking.Id, booking.StartsAt, (int)booking.Duration.TotalMinutes, booking.Status, booking.CreatedAt, booking.ExpiresAt);

    public static BookingResponse From(BookingListItem item) => new(item.Id, item.StartsAt, (int)item.Duration.TotalMinutes, item.Status, item.CreatedAt, item.ExpiresAt);
}