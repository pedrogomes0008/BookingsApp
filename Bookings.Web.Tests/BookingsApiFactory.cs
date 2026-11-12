using Bookings.Core.Application.Bookings;
using Bookings.Tests.Shared.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bookings.Web.Tests;

public class BookingsApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IBookingRepository>();

            services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
        });
    }
}