using full.API.Data;
using full.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace full.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : Controller
    {
        private readonly FullDbContext _fullDbContext;
        public EmployeesController(FullDbContext fullDbContext)
        {
            _fullDbContext = fullDbContext;
        }
        [HttpGet]
        public async Task<ActionResult> GetAllEmployees()
        {
            var employees = await _fullDbContext.Employees.ToListAsync();
            return Ok(employees);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody] Employee employeeRequest)
        {
            employeeRequest.Id = Guid.NewGuid();
            await _fullDbContext.Employees.AddAsync(employeeRequest);
            await _fullDbContext.SaveChangesAsync();
            return Ok(employeeRequest);
        }

        [HttpGet]
        [Route("{id:Guid}")]

        //[HttpGet("{id:int}")]
        public async Task<IActionResult> GetEmployee([FromRoute] Guid id)
        {
            var employee =
                await _fullDbContext.Employees.FirstOrDefaultAsync((x) => x.Id == id);

            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);

        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> UpdateEmployee([FromRoute] Guid id, Employee updateEmployeeRequest)
        {
            
            var employee = await _fullDbContext.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }
            employee.Name = updateEmployeeRequest.Name;
            employee.Email = updateEmployeeRequest.Email;
            employee.Salary = updateEmployeeRequest.Salary;
            employee.Phone = updateEmployeeRequest.Phone;
            employee.Department = updateEmployeeRequest.Department;

           await _fullDbContext.SaveChangesAsync();

            return Ok(employee);

        }

        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> DeleteEmployee([FromRoute] Guid id)
        {
            var employee = await _fullDbContext.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            _fullDbContext.Employees.Remove(employee);
            await _fullDbContext.SaveChangesAsync();

            return Ok(employee);
        }

    }
}
