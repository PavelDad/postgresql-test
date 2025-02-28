using EmployeesApi.Data;
using Microsoft.EntityFrameworkCore;
using EmployeesApi.Models;
using EmployeesApi.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddSingleton<AuthService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Добавление поддержки авторизации через JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Введите JWT токен в формате: Bearer <token>"
    });

    // Глобальное применение авторизации ко всем эндпоинтам
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Employees API V1");
    options.RoutePrefix = string.Empty;
});

// Middleware для обработки запросов
app.UseAuthentication(); // Добавляем аутентификацию
app.UseAuthorization();  // Добавляем авторизацию

app.MapGet("/employees", async (IEmployeeService service) =>
{
    var employees = await service.GetAllAsync();
    return Results.Ok(employees);
}).RequireAuthorization();

app.MapGet("/employees/{id}", async (Guid id, IEmployeeService service) =>
{
    var employee = await service.GetByIdAsync(id);
    return employee == null ? Results.NotFound() : Results.Ok(employee);
}).RequireAuthorization();

app.MapPost("/employees", async (Employee employee, IEmployeeService service) =>
{
    var createdEmployee = await service.CreateAsync(employee);
    return Results.Created($"/employees/{createdEmployee.Id}", createdEmployee);
}).RequireAuthorization();

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
}).RequireAuthorization();

app.MapDelete("/employees/{id}", async (Guid id, IEmployeeService service) =>
{
    await service.DeleteAsync(id);
    return Results.NoContent();
}).RequireAuthorization();

app.MapPost("/register", async (User user, AppDbContext context) =>
{
    // Хэширование пароля (упрощённо)
    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

    context.Users.Add(user);
    await context.SaveChangesAsync();
    return Results.Ok("User registered successfully");
}).AllowAnonymous();

app.MapPost("/login", async (LoginRequest login, AppDbContext context, AuthService authService) =>
{
    await CheckAndSetDefaultUser(context);

    var user = await context.Users.FirstOrDefaultAsync(u => u.Username == login.Username);
    if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var token = authService.GenerateToken(user);
    return Results.Ok(new { Token = token });
}).AllowAnonymous();

app.Run();

async Task CheckAndSetDefaultUser(AppDbContext context)
{
    if (!await context.Users.AnyAsync())
    {
        var defaultUser = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!")
        };
        context.Users.Add(defaultUser);
        await context.SaveChangesAsync();
    }
}

public record LoginRequest(string Username, string Password);
