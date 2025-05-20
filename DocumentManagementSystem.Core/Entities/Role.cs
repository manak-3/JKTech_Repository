using Microsoft.AspNetCore.Identity;
namespace DocumentManagementSystem.Core.Entities;

public class Role : IdentityRole
{
    public string Description { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
}