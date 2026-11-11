using System.ComponentModel.DataAnnotations;

namespace Bookings.Web.Endpoints;

public record CreateBookingRequest(
    [Required] DateTimeOffset? StartsAt,
    [Required, Range(1, 1440)] int? DurationMinutes);