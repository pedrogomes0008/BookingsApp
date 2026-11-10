using Bookings.Core.Domain.Bookings;
using Microsoft.Extensions.Options;

namespace Bookings.Core.Application.Bookings;

public class BookingRequestService
{
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _maxApprovalWindow;
    private readonly IBookingRepository _bookingRepository;

    public BookingRequestService(
        TimeProvider timeProvider,
        IBookingRepository bookingRepository,
        IOptions<BookingSettings> options)
    {
        _timeProvider = timeProvider;
        _bookingRepository = bookingRepository;
        _maxApprovalWindow = TimeSpan.FromHours(options.Value.MaxApprovalWindowHours);
    }

    public async Task<Booking> RequestAsync(DateTimeOffset startsAt, TimeSpan duration, CancellationToken cancellationToken)
    {
        var booking = Booking.Request(startsAt, duration, _timeProvider.GetUtcNow(), _maxApprovalWindow);

        await _bookingRepository.AddAsync(booking, cancellationToken);

        return booking;
    }
}