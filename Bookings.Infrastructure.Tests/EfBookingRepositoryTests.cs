using Bookings.Core.Domain.Bookings;
using Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Tests;

public class EfBookingRepositoryTests
{
    private const string ConnectionString = "Server=.\\SQLEXPRESS;Database=BookingsApp_Tests;Trusted_Connection=True;TrustServerCertificate=True";

    private static BookingsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseSqlServer(ConnectionString).Options;

        return new BookingsDbContext(options);
    }

    public EfBookingRepositoryTests()
    {
        // 1. create a context
        using var db = CreateContext();
        // 2. delete the test database if it exists
        db.Database.EnsureDeleted();
        // 3. create it again by applying the migrations
        db.Database.Migrate();
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        // Arrange
        await using var context = CreateContext();
        var bookingRepository = new EfBookingRepository(context);
        var guid = Guid.NewGuid();

        // Act
        var result = await bookingRepository.GetByIdAsync(guid, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsSavedBooking()
    {
        // Arrange
        await using var context = CreateContext();
        var writeRepository = new EfBookingRepository(context);

        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var startsAt = now.AddDays(1);
        var duration = TimeSpan.FromMinutes(60);
        var booking = Booking.Request(startsAt, duration, now, TimeSpan.FromHours(24));

        // Act
        await writeRepository.AddAsync(booking, CancellationToken.None);

        // Assert
        await using var readContext = CreateContext();
        var readRepository = new EfBookingRepository(readContext);
        var result = await readRepository.GetByIdAsync(booking.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(booking.StartsAt, result.StartsAt);
        Assert.Equal(booking.Duration, result.Duration);
        Assert.Equal(booking.Status, result.Status);
        Assert.Equal(booking.CreatedAt, result.CreatedAt);
        Assert.Equal(booking.ExpiresAt, result.ExpiresAt);
    }

    [Fact]
    public async Task SaveAsync_PersistsStatusChange()
    {
        // Arrange
        await using var context = CreateContext();
        var writeRepository = new EfBookingRepository(context);
        var now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var startsAt = now.AddDays(1);
        var duration = TimeSpan.FromMinutes(60);
        var booking = Booking.Request(startsAt, duration, now, TimeSpan.FromHours(24));
        await writeRepository.AddAsync(booking, CancellationToken.None);

        // Act
        await using var approvalContext = CreateContext();
        var readRepository = new EfBookingRepository(approvalContext);
        var result = await readRepository.GetByIdAsync(booking.Id, CancellationToken.None);
        Assert.NotNull(result);
        result.Approve();
        await readRepository.SaveAsync(result, CancellationToken.None);

        // Assert
        await using var readSecondTimeContext = CreateContext();
        var readSecondTimeRepository = new EfBookingRepository(readSecondTimeContext);
        var secondResult = await readSecondTimeRepository.GetByIdAsync(booking.Id, CancellationToken.None);

        Assert.NotNull(secondResult);
        Assert.Equal(BookingStatus.Confirmed, secondResult.Status);
    }
}
