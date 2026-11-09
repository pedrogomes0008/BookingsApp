using Bookings.Core.Application.Bookings;

namespace Bookings.Web.Endpoints;

public static class BookingEndpoints
{
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings");

        group.MapPost("/{id:guid}/approve", async (Guid id, BookingActionService service, CancellationToken cancellationToken) =>
        {
            await service.ApproveAsync(id, cancellationToken);
            return TypedResults.NoContent();
        });

        return app;
    }
}