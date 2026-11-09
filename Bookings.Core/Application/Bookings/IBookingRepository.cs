using Bookings.Core.Domain.Bookings;

namespace Bookings.Core.Application.Bookings;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Booking booking, CancellationToken cancellationToken);

    Task SaveAsync(Booking booking, CancellationToken cancellationToken);
}