using System.ComponentModel.DataAnnotations;

namespace full.API.Contracts;

/// <summary>Filtering and paging options for listing employees.</summary>
public record EmployeeQuery
{
    /// <summary>Case-insensitive match against name or email.</summary>
    public string? Search { get; init; }

    /// <summary>Exact department name.</summary>
    public string? Department { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 50;
}
