using System.Text.Json.Serialization;
using Bookings.Core.Application.Bookings;
using Bookings.Web.ErrorHandling;

namespace Bookings.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddOptions<BookingSettings>()
            .Bind(configuration.GetSection("Bookings"))
            .Validate(settings => settings.MaxApprovalWindowHours > 0,
                    "Bookings:MaxApprovalWindowHours must be greater than zero.")
            .ValidateOnStart();

        services.ConfigureHttpJsonOptions(options =>
                                          options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddProblemDetails();
        services.AddExceptionHandler<BookingExceptionHandler>();

        return services;
    }
}