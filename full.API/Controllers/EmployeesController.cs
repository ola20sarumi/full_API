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
        [Route("(id:Guid)")]
        public async Task<IActionResult> GetEmployee([FromRoute] Guid id)
        {
            var employee = 
                await _fullDbContext.Employees.FirstOrDefaultAsync(x => x.Id == id);

            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);

        }
    }
}
