using DocumentManagementSystem.API.Services;
using DocumentManagementSystem.Core.Dtos.AuthDtos;
using DocumentManagementSystem.Core.Entities;
using DocumentManagementSystem.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

public class AuthServiceTests
{
	private readonly Mock<UserManager<User>> _mockUserManager;
	private readonly Mock<RoleManager<Role>> _mockRoleManager;
	private readonly Mock<IConfiguration> _mockConfiguration;
	private readonly AuthService _authService;

	public AuthServiceTests()
	{
		_mockUserManager = MockUserManager();
		_mockRoleManager = MockRoleManager();
		_mockConfiguration = new Mock<IConfiguration>();

		var jwtSection = new Mock<IConfigurationSection>();
		_mockConfiguration.Setup(c => c["JwtSettings:secretKey"]).Returns("my_super_secret_32_byte_long_key__");
		_mockConfiguration.Setup(c => c["JwtSettings:validIssuer"]).Returns("TestIssuer");
		_mockConfiguration.Setup(c => c["JwtSettings:validAudience"]).Returns("TestAudience");


		_mockConfiguration.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSection.Object);

		_authService = new AuthService(
			_mockUserManager.Object,
			_mockRoleManager.Object,
			_mockConfiguration.Object
		);
	}

	[Fact]
	public async Task Login_ReturnsToken_WhenCredentialsAreValid()
	{
		var email = "test@example.com";
		var password = "Password123!";
		var user = new User
		{
			Id = "user-id-123",
			UserName = "testuser",
			Email = email,
			FirstName = "Test",
			LastName = "User"
		};

		_mockUserManager.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
		_mockUserManager.Setup(um => um.CheckPasswordAsync(user, password)).ReturnsAsync(true);
		_mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { RoleType.User.ToString() });

		var loginDto = new LoginDto { Email = email, Password = password };

		var result = await _authService.Login(loginDto);

		Assert.NotNull(result);
		Assert.Equal(user.Id, result.UserId);
		Assert.Equal(email, result.Email);
		Assert.Contains(RoleType.User.ToString(), result.Roles);
		Assert.False(string.IsNullOrWhiteSpace(result.Token));
		Assert.True(result.Expiration > DateTime.UtcNow);
	}

	[Fact]
	public async Task Login_ThrowsUnauthorized_WhenUserNotFound()
	{
		_mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null);

		var loginDto = new LoginDto { Email = "nouser@example.com", Password = "pass" };

		await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(loginDto));
	}

	[Fact]
	public async Task Login_ThrowsUnauthorized_WhenPasswordInvalid()
	{
		var user = new User { Id = "id", UserName = "name", Email = "email" };
		_mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
		_mockUserManager.Setup(um => um.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);

		var loginDto = new LoginDto { Email = "email", Password = "wrongpass" };

		await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(loginDto));
	}

	[Fact]
	public async Task Register_ReturnsSuccess_WhenUserCreated()
	{
		var registerDto = new RegisterDto
		{
			Email = "newuser@example.com",
			FirstName = "New",
			LastName = "User",
			Password = "StrongPass123!"
		};

		_mockUserManager.Setup(um => um.FindByEmailAsync(registerDto.Email)).ReturnsAsync((User)null);
		_mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), registerDto.Password))
			.ReturnsAsync(IdentityResult.Success);
		_mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), RoleType.User.ToString()))
			.ReturnsAsync(IdentityResult.Success);

		var result = await _authService.Register(registerDto);

		Assert.True(result.Succeeded);
		_mockUserManager.Verify(um => um.CreateAsync(It.IsAny<User>(), registerDto.Password), Times.Once);
		_mockUserManager.Verify(um => um.AddToRoleAsync(It.IsAny<User>(), RoleType.User.ToString()), Times.Once);
	}

	[Fact]
	public async Task Register_Throws_WhenUserAlreadyExists()
	{
		var registerDto = new RegisterDto { Email = "exists@example.com" };
		_mockUserManager.Setup(um => um.FindByEmailAsync(registerDto.Email)).ReturnsAsync(new User());

		await Assert.ThrowsAsync<ApplicationException>(() => _authService.Register(registerDto));
	}

	[Fact]
	public async Task Register_Throws_WhenCreateFails()
	{
		var registerDto = new RegisterDto
		{
			Email = "fail@example.com",
			Password = "pass",
			FirstName = "Fail",
			LastName = "User"
		};

		_mockUserManager.Setup(um => um.FindByEmailAsync(registerDto.Email)).ReturnsAsync((User)null);
		_mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), registerDto.Password))
			.ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

		await Assert.ThrowsAsync<ApplicationException>(() => _authService.Register(registerDto));
	}

	// Helpers to mock UserManager and RoleManager (required because no parameterless constructors)

	private static Mock<UserManager<User>> MockUserManager()
	{
		var store = new Mock<IUserStore<User>>();
		return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
	}

	private static Mock<RoleManager<Role>> MockRoleManager()
	{
		var store = new Mock<IRoleStore<Role>>();
		var roles = new List<Role>();
		return new Mock<RoleManager<Role>>(store.Object, null, null, null, null);
	}
}
