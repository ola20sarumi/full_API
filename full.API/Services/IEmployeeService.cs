using full.API.Contracts;

namespace full.API.Services;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeResponse>> ListAsync(EmployeeQuery query, CancellationToken ct);
    Task<EmployeeResponse?> GetAsync(Guid id, CancellationToken ct);

    /// <exception cref="DuplicateEmailException">Another employee already uses the email.</exception>
    Task<EmployeeResponse> CreateAsync(EmployeeRequest request, CancellationToken ct);

    /// <returns>The updated employee, or <c>null</c> if it does not exist.</returns>
    /// <exception cref="DuplicateEmailException">Another employee already uses the email.</exception>
    Task<EmployeeResponse?> UpdateAsync(Guid id, EmployeeRequest request, CancellationToken ct);

    /// <returns><c>false</c> if the employee does not exist.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
