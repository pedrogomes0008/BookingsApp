using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bookings.Core.Domain.Bookings;
using Bookings.Web.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        var location = await CreatePendingBookingAsync(client);

        // Act
        var approveResponse = await client.PostAsync($"{location}/approve", null);
        Assert.Equal(HttpStatusCode.NoContent, approveResponse.StatusCode);

        // Assert
        var bookingResponse = await client.GetFromJsonAsync<BookingResponse>(location, JsonOptions);
        Assert.NotNull(bookingResponse);
        Assert.Equal(BookingStatus.Confirmed, bookingResponse.Status);
    }

    [Fact]
    public async Task Approve_UnexistingBooking_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var bookingId = Guid.NewGuid();

        // Act
        var response = await client.PostAsync($"/api/bookings/{bookingId}/approve", null);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
    }

    [Fact]
    public async Task Approve_ApprovedBooking_ReturnsConflict()
    {
        // Arrange
        var client = _factory.CreateClient();
        var location = await CreatePendingBookingAsync(client);

        var approveResponse = await client.PostAsync($"{location}/approve", null);
        Assert.Equal(HttpStatusCode.NoContent, approveResponse.StatusCode);

        // Act
        var approveAgain = await client.PostAsync($"{location}/approve", null);
        var problemDetails = await approveAgain.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, approveAgain.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
    }

    private static async Task<Uri> CreatePendingBookingAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/bookings",
                    new
                    {
                        startsAt = DateTimeOffset.UtcNow.AddDays(2),
                        durationMinutes = 60
                    });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);

        return location;
    }
}