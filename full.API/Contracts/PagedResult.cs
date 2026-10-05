namespace full.API.Contracts;

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);
