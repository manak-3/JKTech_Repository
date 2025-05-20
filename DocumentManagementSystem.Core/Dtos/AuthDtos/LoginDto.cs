using System.ComponentModel.DataAnnotations;

namespace DocumentManagementSystem.Core.Dtos.AuthDtos;
public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
