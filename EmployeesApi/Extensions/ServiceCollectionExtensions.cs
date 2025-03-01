using EmployeesApi.Data;
using EmployeesApi.Services;
using Microsoft.EntityFrameworkCore;

namespace EmployeesApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Регистрация контекста базы данных
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Регистрация сервисов
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddSingleton<AuthService>();
    }
}