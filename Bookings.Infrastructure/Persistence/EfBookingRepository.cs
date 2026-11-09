using Bookings.Core.Application.Bookings;
using Bookings.Core.Domain.Bookings;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Persistence;

public class EfBookingRepository : IBookingRepository
{
    private readonly BookingsDbContext _bookingsDbContext;

    public EfBookingRepository(BookingsDbContext bookingsDbContext)
    {
        _bookingsDbContext = bookingsDbContext;
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken)
    {
        _bookingsDbContext.Bookings.Add(booking);

        await _bookingsDbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _bookingsDbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task SaveAsync(Booking booking, CancellationToken cancellationToken)
    {
        await _bookingsDbContext.SaveChangesAsync(cancellationToken);
    }
}