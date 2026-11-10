using Bookings.Core.Application.Bookings;
using Microsoft.AspNetCore.Http.HttpResults;

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

        group.MapPost("/{id:guid}/reject", async (Guid id, BookingActionService service, CancellationToken cancellationToken) =>
        {
            await service.RejectAsync(id, cancellationToken);
            return TypedResults.NoContent();
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, BookingActionService service, CancellationToken cancellationToken) =>
        {
            await service.CancelAsync(id, cancellationToken);
            return TypedResults.NoContent();
        });

        group.MapPost("/{id:guid}/no-show", async (Guid id, BookingActionService service, CancellationToken cancellationToken) =>
        {
            await service.MarkNoShowAsync(id, cancellationToken);
            return TypedResults.NoContent();
        });

        group.MapPost("/{id:guid}/reschedule", async (Guid id, RescheduleBookingRequest request, BookingActionService service, CancellationToken cancellationToken) =>
        {
            await service.RescheduleAsync(id, request.NewStartsAt, cancellationToken);
            return TypedResults.NoContent();
        });

        group.MapGet("/{id:guid}", async Task<Results<Ok<BookingResponse>, NotFound>> (Guid id, IBookingRepository repository, CancellationToken cancellationToken) =>
        {
            var booking = await repository.GetByIdAsync(id, cancellationToken);

            if (booking is null) return TypedResults.NotFound();

            return TypedResults.Ok(BookingResponse.From(booking));
        });

        return app;
    }
}