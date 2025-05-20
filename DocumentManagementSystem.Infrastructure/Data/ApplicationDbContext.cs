using DocumentManagementSystem.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DocumentManagementSystem.Infrastructure.Data;
public class ApplicationDbContext : IdentityDbContext<User, Role, string, 
    IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, 
    IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentMetadata> DocumentMetadata { get; set; }
	public DbSet<IngestionStatus> IngestionStatuses { get; set; }
	public DbSet<Role> Roles { get; set; }
	public DbSet<User> Users { get; set; }
	public DbSet<UserRole> UserRoles { get; set; }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		builder.Entity<Role>(role =>
		{
			role.Property(r => r.Description)
				.HasMaxLength(256);
		});

		builder.Entity<UserRole>(userRole =>
		{
			userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

			userRole.HasOne(ur => ur.Role)
				.WithMany(r => r.UserRoles)
				.HasForeignKey(ur => ur.RoleId)
				.IsRequired();

			userRole.HasOne(ur => ur.User)
				.WithMany(u => u.UserRoles)
				.HasForeignKey(ur => ur.UserId)
				.IsRequired();
		});

		builder.Entity<Document>(document =>
		{
			document.HasOne(d => d.UploadedByUser)
				.WithMany(u => u.Documents)
				.HasForeignKey(d => d.UploadedByUserId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		builder.Entity<DocumentMetadata>(metadata =>
		{
			metadata.HasOne(m => m.Document)
				.WithMany(d => d.Metadata)
				.HasForeignKey(m => m.DocumentId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		builder.Entity<IngestionStatus>(status =>
		{
			status.HasKey(s => s.Id);

			status.HasOne<Document>()
				.WithMany()
				.HasForeignKey(s => s.DocumentId)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}
}