using System.Text.Json.Serialization;
using Bookings.Web.ErrorHandling;

namespace Bookings.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
                                          options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddProblemDetails();
        services.AddExceptionHandler<BookingExceptionHandler>();

        return services;
    }
}