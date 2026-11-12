namespace Bookings.Web.Endpoints.Common;

public record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);