using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bookings.Core.Domain.Bookings;
using Bookings.Web.Endpoints;
using Microsoft.AspNetCore.Http;

namespace Bookings.Web.Tests;

public class BookingWebApplicationTests : IClassFixture<BookingsApiFactory>
{
    private readonly BookingsApiFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public BookingWebApplicationTests(BookingsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostBooking_EmptyBody_ReturnsValidationErrors()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/bookings", new { });
        var problemDetails = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Contains("StartsAt", problemDetails.Errors);
        Assert.Contains("DurationMinutes", problemDetails.Errors);
    }

    [Fact]
    public async Task Approve_PendingBooking_ConfirmsBooking()
    {
        // Arrange
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/bookings",
                    new
                    {
                        startsAt = DateTimeOffset.UtcNow.AddDays(2),
                        durationMinutes = 60
                    });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var location = response.Headers.Location;
        Assert.NotNull(location);

        // Act
        var approveResponse = await client.PostAsync($"{location}/approve", null);
        Assert.Equal(HttpStatusCode.NoContent, approveResponse.StatusCode);

        // Assert
        var bookingResponse = await client.GetFromJsonAsync<BookingResponse>(location, JsonOptions);
        Assert.NotNull(bookingResponse);
        Assert.Equal(BookingStatus.Confirmed, bookingResponse.Status);
    }
}