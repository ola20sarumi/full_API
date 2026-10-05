using full.API.Contracts;
using full.API.Data;
using full.API.Models;
using Microsoft.EntityFrameworkCore;

namespace full.API.Services;

public class EmployeeService(FullDbContext db) : IEmployeeService
{
    public async Task<PagedResult<EmployeeResponse>> ListAsync(EmployeeQuery query, CancellationToken ct)
    {
        var employees = db.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            employees = employees.Where(e => EF.Functions.Like(e.Name, pattern) || EF.Functions.Like(e.Email, pattern));
        }

        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            var department = query.Department.Trim();
            employees = employees.Where(e => e.Department == department);
        }

        var total = await employees.CountAsync(ct);
        var items = await employees
            .OrderBy(e => e.Name).ThenBy(e => e.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new EmployeeResponse(e.Id, e.Name, e.Email, e.Phone, e.Salary, e.Department))
            .ToListAsync(ct);

        return new PagedResult<EmployeeResponse>(items, total);
    }

    public async Task<EmployeeResponse?> GetAsync(Guid id, CancellationToken ct)
    {
        var employee = await db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        return employee is null ? null : EmployeeResponse.FromEntity(employee);
    }

    public async Task<EmployeeResponse> CreateAsync(EmployeeRequest request, CancellationToken ct)
    {
        var email = NormalizeEmail(request.Email);
        await EnsureEmailIsFreeAsync(email, excludingId: null, ct);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            Salary = request.Salary,
            Department = request.Department.Trim()
        };

        db.Employees.Add(employee);
        await db.SaveChangesAsync(ct);
        return EmployeeResponse.FromEntity(employee);
    }

    public async Task<EmployeeResponse?> UpdateAsync(Guid id, EmployeeRequest request, CancellationToken ct)
    {
        var employee = await db.Employees.FindAsync([id], ct);
        if (employee is null)
        {
            return null;
        }

        var email = NormalizeEmail(request.Email);
        await EnsureEmailIsFreeAsync(email, excludingId: id, ct);

        employee.Name = request.Name.Trim();
        employee.Email = email;
        employee.Phone = request.Phone.Trim();
        employee.Salary = request.Salary;
        employee.Department = request.Department.Trim();

        await db.SaveChangesAsync(ct);
        return EmployeeResponse.FromEntity(employee);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var deleted = await db.Employees.Where(e => e.Id == id).ExecuteDeleteAsync(ct);
        return deleted > 0;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    // The unique index is the real guarantee; this check gives callers a clear 409
    // instead of a generic database error in the common case.
    private async Task EnsureEmailIsFreeAsync(string email, Guid? excludingId, CancellationToken ct)
    {
        var taken = await db.Employees.AnyAsync(e => e.Email == email && e.Id != excludingId, ct);
        if (taken)
        {
            throw new DuplicateEmailException(email);
        }
    }
}
