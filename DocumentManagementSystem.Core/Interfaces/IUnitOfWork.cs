using DocumentManagementSystem.Core.Entities;
using Microsoft.AspNetCore.Identity;
namespace DocumentManagementSystem.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> UserRepository { get; }
	IRepository<IngestionStatus> IngestionStatusRepository { get; }
	IRepository<Document> DocumentRepository { get; }
    IRepository<DocumentMetadata> DocumentMetadataRepository { get; }
    UserManager<User> UserManager { get; }
    RoleManager<Role> RoleManager { get; }
    Task<int> CompleteAsync();
}