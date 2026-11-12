using Bookings.Core.Application.Bookings;
using Bookings.Core.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Persistence;

public class EfBookingQueries : IBookingQueries
{
    private readonly BookingsDbContext _bookingsDbContext;

    public EfBookingQueries(BookingsDbContext bookingsDbContext)
    {
        _bookingsDbContext = bookingsDbContext;
    }

    public async Task<PagedResult<BookingListItem>> ListBookingsAsync(BookingListQuery query, CancellationToken cancellationToken)
    {
        var bookings = _bookingsDbContext.Bookings.AsNoTracking();

        if (query.Status is not null)
        {
            bookings = bookings.Where(b => b.Status == query.Status);
        }

        if (query.From is not null)
        {
            bookings = bookings.Where(b => b.StartsAt >= query.From);
        }

        if (query.To is not null)
        {
            bookings = bookings.Where(b => b.StartsAt < query.To);
        }

        var totalCount = await bookings.CountAsync(cancellationToken);
        var items = await bookings
            .OrderBy(b => b.StartsAt)
            .ThenBy(b => b.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => new BookingListItem(b.Id, b.StartsAt, b.Duration, b.Status, b.CreatedAt, b.ExpiresAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<BookingListItem>(items, query.Page, query.PageSize, totalCount);
    }
}