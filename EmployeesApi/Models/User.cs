using System.ComponentModel.DataAnnotations;

namespace EmployeesApi.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Username { get; set; }

    [Required]
    public string PasswordHash { get; set; } // Хэшированный пароль
}