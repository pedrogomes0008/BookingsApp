using Bookings.Core.Domain.Bookings;

namespace Bookings.Core.Application.Bookings;

public record BookingListQuery(
    BookingStatus? Status,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize);