using EmployeesApi.Models;
using EmployeesApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _employeeService.GetAllAsync();
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Employee employee)
    {
        var createdEmployee = await _employeeService.CreateAsync(employee);
        return CreatedAtAction(nameof(GetById), new { id = createdEmployee.Id }, createdEmployee);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Employee updatedEmployee)
    {
        var existingEmployee = await _employeeService.GetByIdAsync(id);
        if (existingEmployee is null)
            return NotFound();

        updatedEmployee.Id = id;
        await _employeeService.UpdateAsync(updatedEmployee);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _employeeService.DeleteAsync(id);
        return NoContent();
    }
}
