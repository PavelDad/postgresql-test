using EmployeesApi.Data;
using EmployeesApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeesApi.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;

        public EmployeeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees.OrderBy(x => x.Name.ToLower()).ToListAsync();
        }

        public async Task<Employee> GetByIdAsync(Guid id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task<Employee> CreateAsync(Employee employee)
        {
            // Игнорируем переданный Id (он будет сгенерирован автоматически)
            employee.Id = Guid.Empty; // Очищаем Id, чтобы база данных сгенерировала его

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task UpdateAsync(Employee updatedEmployee)
        {
            // Находим существующую запись в базе данных
            var existingEmployee = await _context.Employees.FindAsync(updatedEmployee.Id);
            if (existingEmployee == null)
            {
                throw new ArgumentException("Employee not found");
            }

            // Обновляем свойства существующей записи
            existingEmployee.Name = updatedEmployee.Name;
            existingEmployee.BirthDate = updatedEmployee.BirthDate;

            // Сохраняем изменения
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }
    }
}