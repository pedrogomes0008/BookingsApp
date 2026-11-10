using Bookings.Core.Application.Bookings;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<BookingActionService>();
        services.AddScoped<BookingRequestService>();

        return services;
    }
}