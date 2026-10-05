using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using full.API.Infrastructure;
using full.API.Models;

namespace full.API.Contracts;

/// <summary>Payload for creating or updating an employee.</summary>
public record EmployeeRequest
{
    /// <example>Jane Doe</example>
    [Required, StringLength(EmployeeLimits.NameMaxLength, MinimumLength = 2)]
    public required string Name { get; init; }

    /// <example>jane.doe@example.com</example>
    [Required, EmailAddress, StringLength(EmployeeLimits.EmailMaxLength)]
    public required string Email { get; init; }

    /// <summary>Digits with an optional leading '+', spaces, dashes or parentheses. Numeric JSON values are accepted.</summary>
    /// <example>+44 7700 900123</example>
    [Required, StringLength(EmployeeLimits.PhoneMaxLength, MinimumLength = 7)]
    [RegularExpression(@"^\+?[0-9 ()\-]+$", ErrorMessage = "Phone may only contain digits, spaces, dashes, parentheses and a leading '+'.")]
    [JsonConverter(typeof(StringOrNumberJsonConverter))]
    public required string Phone { get; init; }

    /// <example>55000</example>
    [Range(typeof(decimal), "0", "10000000")]
    public decimal Salary { get; init; }

    /// <example>Engineering</example>
    [Required, StringLength(EmployeeLimits.DepartmentMaxLength, MinimumLength = 2)]
    public required string Department { get; init; }
}
