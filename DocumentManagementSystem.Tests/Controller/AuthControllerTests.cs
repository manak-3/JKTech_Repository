using DocumentManagementSystem.Core.Dtos.AuthDtos;
using DocumentManagementSystem.API.Interfaces;
using DocumentManagementSystem.API.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class AuthControllerTests
{
	private readonly Mock<IAuthService> _authServiceMock;
	private readonly AuthController _controller;

	public AuthControllerTests()
	{
		_authServiceMock = new Mock<IAuthService>();
		_controller = new AuthController(_authServiceMock.Object);
	}

	[Fact]
	public async Task Register_ValidRequest_ReturnsOkResult()
	{
		// Arrange
		var registerDto = new RegisterDto { Email = "test@example.com", Password = "Test123!" };
		var identityResult = IdentityResult.Success;

		_authServiceMock.Setup(s => s.Register(registerDto)).ReturnsAsync(identityResult);

		// Act
		var result = await _controller.Register(registerDto);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		Assert.Equal(200, okResult.StatusCode);
	}

	[Fact]
	public async Task Register_InvalidRequest_ReturnsBadRequest()
	{
		// Arrange
		var registerDto = new RegisterDto { Email = "test@example.com", Password = "weak" };
		var identityResult = IdentityResult.Failed(new IdentityError { Description = "Password too weak" });

		_authServiceMock.Setup(s => s.Register(registerDto)).ReturnsAsync(identityResult);

		// Act
		var result = await _controller.Register(registerDto);

		// Assert
		var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
		Assert.Equal(400, badRequestResult.StatusCode);
	}

	[Fact]
	public async Task Login_ValidRequest_ReturnsOkResultWithToken()
	{
		// Arrange
		var loginDto = new LoginDto { Email = "test@example.com", Password = "Test123!" };
		var authResponse = new AuthResponseDto { Token = "jwt-token", Expiration = DateTime.UtcNow.AddHours(1) };

		_authServiceMock.Setup(s => s.Login(loginDto)).ReturnsAsync(authResponse);

		// Act
		var result = await _controller.Login(loginDto);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		var response = Assert.IsType<AuthResponseDto>(okResult.Value);
		Assert.Equal("jwt-token", response.Token);
	}

	[Fact]
	public async Task Login_InvalidCredentials_ReturnsOkWithNullTokenOrError()
	{
		// Arrange
		var loginDto = new LoginDto { Email = "wrong@example.com", Password = "wrong" };
		AuthResponseDto authResponse = null;

		_authServiceMock.Setup(s => s.Login(loginDto)).ReturnsAsync(authResponse);

		// Act
		var result = await _controller.Login(loginDto);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		Assert.Null(okResult.Value);
	}
}
