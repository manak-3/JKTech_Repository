namespace DocumentManagementSystem.Core.Entities;

public class Document
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string Name { get; set; }
    public string Description { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    public string UploadedByUserId { get; set; }
    
    // Navigation properties
    public User UploadedByUser { get; set; }
    public ICollection<DocumentMetadata> Metadata { get; set; }
}