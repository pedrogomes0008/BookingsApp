using System.ComponentModel.DataAnnotations;
using Bookings.Core.Domain.Bookings;

namespace Bookings.Web.Endpoints;

public record ListBookingsRequest(
    BookingStatus? Status,
    DateTimeOffset? From,
    DateTimeOffset? To,
    [Range(1, int.MaxValue)] int Page = 1,
    [Range(1, 100)] int PageSize = 20
);