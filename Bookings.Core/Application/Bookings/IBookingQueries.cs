using Bookings.Core.Application.Common;

namespace Bookings.Core.Application.Bookings;

public interface IBookingQueries
{
    Task<PagedResult<BookingListItem>> ListBookingsAsync(BookingListQuery listQuery, CancellationToken cancellationToken);
}