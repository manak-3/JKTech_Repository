using Microsoft.AspNetCore.Identity;
namespace DocumentManagementSystem.Core.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<Document> Documents { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
}