using EmployeesApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Настройка сервисов
builder.Services.AddAppServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

// Добавление контроллеров
builder.Services.AddControllers();

var app = builder.Build();

// Настройка middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Employees API V1");
    options.RoutePrefix = string.Empty;
});

app.UseAuthentication();
app.UseAuthorization();

// Маршрутизация через контроллеры
app.MapControllers();

app.Run();