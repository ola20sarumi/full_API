using System.Net.Mime;
using full.API.Contracts;
using full.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace full.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController(IEmployeeService employees) : ControllerBase
{
    /// <summary>Lists employees, optionally filtered by search text or department.</summary>
    /// <remarks>The total number of matches is returned in the <c>X-Total-Count</c> header.</remarks>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EmployeeResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<EmployeeResponse>>> List([FromQuery] EmployeeQuery query, CancellationToken ct)
    {
        var result = await employees.ListAsync(query, ct);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result.Items);
    }

    /// <summary>Gets a single employee.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> Get(Guid id, CancellationToken ct)
    {
        var employee = await employees.GetAsync(id, ct);
        return employee is null ? NotFound() : Ok(employee);
    }

    /// <summary>Creates an employee.</summary>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeResponse>> Create(EmployeeRequest request, CancellationToken ct)
    {
        var employee = await employees.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = employee.Id }, employee);
    }

    /// <summary>Replaces an employee's details.</summary>
    [HttpPut("{id:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeResponse>> Update(Guid id, EmployeeRequest request, CancellationToken ct)
    {
        var employee = await employees.UpdateAsync(id, request, ct);
        return employee is null ? NotFound() : Ok(employee);
    }

    /// <summary>Deletes an employee.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await employees.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
