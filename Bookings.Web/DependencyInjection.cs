using Bookings.Web.ErrorHandling;

namespace Bookings.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<BookingExceptionHandler>();

        return services;
    }
}