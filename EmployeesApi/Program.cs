using EmployeesApi.Data;
using Microsoft.EntityFrameworkCore;
using EmployeesApi.Models;
using EmployeesApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEmployeeService, EmployeeService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Employees API V1");
    options.RoutePrefix = "Swagger"; // Делает Swagger доступным по корневому пути
});

app.MapGet("/employees", async (IEmployeeService service) =>
{
    var employees = await service.GetAllAsync();
    return Results.Ok(employees);
});

app.MapGet("/employees/{id}", async (Guid id, IEmployeeService service) =>
{
    var employee = await service.GetByIdAsync(id);
    return employee == null ? Results.NotFound() : Results.Ok(employee);
});

app.MapPost("/employees", async (Employee employee, IEmployeeService service) =>
{
    var createdEmployee = await service.CreateAsync(employee);
    return Results.Created($"/employees/{createdEmployee.Id}", createdEmployee);
});

app.MapPut("/employees/{id}", async (Guid id, Employee updatedEmployee, IEmployeeService service) =>
{
    var existingEmployee = await service.GetByIdAsync(id);
    if (existingEmployee == null)
    {
        return Results.NotFound();
    }

    updatedEmployee.Id = id;
    await service.UpdateAsync(updatedEmployee);
    return Results.NoContent();
});

app.MapDelete("/employees/{id}", async (Guid id, IEmployeeService service) =>
{
    await service.DeleteAsync(id);
    return Results.NoContent();
});

app.Run();
