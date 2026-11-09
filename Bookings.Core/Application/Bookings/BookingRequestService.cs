using Bookings.Core.Domain.Bookings;

namespace Bookings.Core.Application.Bookings;

public class BookingRequestService
{
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _maxApprovalWindow;

    private readonly IBookingRepository _bookingRepository;

    public BookingRequestService(
        TimeProvider timeProvider,
        TimeSpan maxApprovalWindow,
        IBookingRepository bookingRepository)
    {
        _timeProvider = timeProvider;
        _maxApprovalWindow = maxApprovalWindow;
        _bookingRepository = bookingRepository;
    }

    public async Task<Booking> RequestAsync(DateTimeOffset startsAt, TimeSpan duration, CancellationToken cancellationToken)
    {
        var booking = Booking.Request(startsAt, duration, _timeProvider.GetUtcNow(), _maxApprovalWindow);

        await _bookingRepository.AddAsync(booking, cancellationToken);

        return booking;
    }
}