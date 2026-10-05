using full.API.Models;

namespace full.API.Contracts;

public record EmployeeResponse(Guid Id, string Name, string Email, string Phone, decimal Salary, string Department)
{
    public static EmployeeResponse FromEntity(Employee e) =>
        new(e.Id, e.Name, e.Email, e.Phone, e.Salary, e.Department);
}
