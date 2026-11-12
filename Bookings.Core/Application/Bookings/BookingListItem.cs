using Bookings.Core.Domain.Bookings;

namespace Bookings.Core.Application.Bookings;

public record BookingListItem(Guid Id, DateTimeOffset StartsAt, TimeSpan Duration, BookingStatus Status, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt);