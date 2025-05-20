using DocumentManagementSystem.Core.Dtos.AuthDtos;
using Microsoft.AspNetCore.Identity;

namespace DocumentManagementSystem.API.Interfaces;
public interface IAuthService
{
    Task<AuthResponseDto> Login(LoginDto loginDto);
    Task<IdentityResult> Register(RegisterDto registerDto);
}