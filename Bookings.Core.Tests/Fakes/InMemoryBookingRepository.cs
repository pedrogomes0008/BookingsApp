using Bookings.Core.Application.Bookings;
using Bookings.Core.Domain.Bookings;

namespace Bookings.Core.Tests.Fakes;

public class InMemoryBookingRepository : IBookingRepository
{
    private readonly Dictionary<Guid, Booking> _bookings = [];

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_bookings.GetValueOrDefault(id));
    }

    public Task AddAsync(Booking booking, CancellationToken cancellationToken)
    {
        _bookings[booking.Id] = booking;

        return Task.CompletedTask;
    }

    public Task SaveAsync(Booking booking, CancellationToken cancellationToken)
    {
        if (_bookings.ContainsKey(booking.Id))
        {
            _bookings[booking.Id] = booking;
        }

        return Task.CompletedTask;
    }
}