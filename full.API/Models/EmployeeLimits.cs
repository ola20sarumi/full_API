namespace full.API.Models;

/// <summary>Column limits shared by the database schema and request validation.</summary>
public static class EmployeeLimits
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 256;
    public const int PhoneMaxLength = 20;
    public const int DepartmentMaxLength = 100;
}
