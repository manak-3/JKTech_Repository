using DocumentManagementSystem.Core.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace DocumentManagementSystem.Infrastructure.Data
{
	public static class DbInitializer
	{
		public static async Task InitializeAsync(ApplicationDbContext context, RoleManager<Role> roleManager)
		{
			// Ensure database and tables are created
			await context.Database.EnsureCreatedAsync();

			// Seed Roles only
			if (!roleManager.Roles.Any())
			{
				var roles = new[]
				{
					new Role
					{
						Id = "11111111-1111-1111-1111-111111111111",
						Name = "Admin",
						NormalizedName = "ADMIN",
						Description = "Administrator role"
					},
					new Role
					{
						Id = "22222222-2222-2222-2222-222222222222",
						Name = "User",
						NormalizedName = "USER",
						Description = "Regular user role"
					},
					new Role
					{
						Id = "33333333-3333-3333-3333-333333333333",
						Name = "Manager",
						NormalizedName = "MANAGER",
						Description = "Manager role"
					}
				};

				foreach (var role in roles)
				{
					await roleManager.CreateAsync(role);
				}
			}
		}
	}
}
