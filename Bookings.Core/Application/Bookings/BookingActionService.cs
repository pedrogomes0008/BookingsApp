using Bookings.Core.Domain.Bookings;

namespace Bookings.Core.Application.Bookings;

public class BookingActionService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly TimeProvider _timeProvider;

    public BookingActionService(
        IBookingRepository bookingRepository,
        TimeProvider timeProvider)
    {
        _bookingRepository = bookingRepository;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Approves a pending booking
    /// </summary>
    /// <param name="id">Booking id</param>
    public async Task ApproveAsync(Guid id, CancellationToken cancellationToken)
    {
        // 1. load the booking
        // 2. if it doesn't exist, throw BookingNotFoundException
        var booking = await GetBookingOrThrowAsync(id, cancellationToken);
        // 3. approve it
        booking.Approve();
        // 4. save
        await _bookingRepository.SaveAsync(booking, cancellationToken);
    }

    /// <summary>
    /// Rejects a pending booking
    /// </summary>
    /// <param name="id">Booking id</param>
    public async Task RejectAsync(Guid id, CancellationToken cancellationToken)
    {
        var booking = await GetBookingOrThrowAsync(id, cancellationToken);

        booking.Reject();

        await _bookingRepository.SaveAsync(booking, cancellationToken);
    }

    /// <summary>
    /// Cancels a confirmed booking
    /// </summary>
    /// <param name="id">Booking Id</param>
    public async Task CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var booking = await GetBookingOrThrowAsync(id, cancellationToken);
        var now = _timeProvider.GetUtcNow();

        booking.Cancel(now);

        await _bookingRepository.SaveAsync(booking, cancellationToken);
    }

    /// <summary>
    /// Marks the booking as a no-show
    /// </summary>
    /// <param name="id">Booking Id</param>
    public async Task MarkNoShowAsync(Guid id, CancellationToken cancellationToken)
    {
        var booking = await GetBookingOrThrowAsync(id, cancellationToken);
        var now = _timeProvider.GetUtcNow();

        booking.MarkNoShow(now);

        await _bookingRepository.SaveAsync(booking, cancellationToken);
    }

    /// <summary>
    /// Reschedules a booking to a new date
    /// </summary>
    /// <param name="id">Booking Id</param>
    /// <param name="newStartsAt">New date</param>
    public async Task RescheduleAsync(Guid id, DateTimeOffset newStartsAt, CancellationToken cancellationToken)
    {
        var booking = await GetBookingOrThrowAsync(id, cancellationToken);
        var now = _timeProvider.GetUtcNow();

        booking.Reschedule(newStartsAt, now);

        await _bookingRepository.SaveAsync(booking, cancellationToken);
    }

    private async Task<Booking> GetBookingOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _bookingRepository.GetByIdAsync(id, cancellationToken) ?? throw new BookingNotFoundException(id);
    }
}