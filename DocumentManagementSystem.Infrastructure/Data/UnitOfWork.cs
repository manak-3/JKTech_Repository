using DocumentManagementSystem.Core.Entities;
using DocumentManagementSystem.Core.Interfaces;
using DocumentManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DocumentManagementSystem.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IRepository<User> _userRepository;
    private IRepository<Document> _documentRepository;
    private IRepository<DocumentMetadata> _documentMetadataRepository;
	private IRepository<IngestionStatus> _ingestionStatusRepository;


	public UnitOfWork(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<Role> roleManager = null)
    {
        _context = context;
        UserManager = userManager;
        RoleManager = roleManager;
    }

    public IRepository<User> UserRepository =>
        _userRepository ??= new Repository<User>(_context);

    public IRepository<Document> DocumentRepository =>
        _documentRepository ??= new Repository<Document>(_context);

    public IRepository<DocumentMetadata> DocumentMetadataRepository =>
        _documentMetadataRepository ??= new Repository<DocumentMetadata>(_context);

	public IRepository<IngestionStatus> IngestionStatusRepository =>
			_ingestionStatusRepository ??= new Repository<IngestionStatus>(_context);

	public UserManager<User> UserManager { get; }
    public RoleManager<Role> RoleManager { get; }

    public async Task<int> CompleteAsync()
    {
        try
        {
			return await _context.SaveChangesAsync();
		}	
        catch
        {
            throw;
        }
	}

    public void Dispose()
    {
        _context.Dispose();
    }
}