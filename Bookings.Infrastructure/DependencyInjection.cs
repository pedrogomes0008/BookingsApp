using Bookings.Core.Application.Bookings;
using Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Bookings")
                               ?? throw new InvalidOperationException("Connection string 'Bookings' not found.");

        services.AddDbContext<BookingsDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IBookingRepository, EfBookingRepository>();
        services.AddScoped<IBookingQueries, EfBookingQueries>();

        return services;
    }
}