namespace DocumentManagementSystem.Core.Dtos.AuthDtos;
public class AuthResponseDto
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public IList<string> Roles { get; set; }
}
