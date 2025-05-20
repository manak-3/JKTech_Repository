namespace DocumentManagementSystem.Core.Dtos.DocumentDtos;
public class DocumentDto
{
    public Guid Id { get; set; } = new Guid();
    public string Name { get; set; }
    public string Description { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; }
    public DateTime? LastModified { get; set; }
    public string UploadedByUserId { get; set; }
    public string UploadedByUserName { get; set; }
    public List<DocumentMetadataDto> Metadata { get; set; }
}
