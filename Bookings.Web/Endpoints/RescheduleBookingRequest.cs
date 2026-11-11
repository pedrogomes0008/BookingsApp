using System.ComponentModel.DataAnnotations;

namespace Bookings.Web.Endpoints;

public record RescheduleBookingRequest(
    [Required] DateTimeOffset? NewStartsAt);