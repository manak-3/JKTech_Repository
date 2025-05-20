namespace DocumentManagementSystem.Core.Dtos.DocumentDtos;
public class UpdateDocumentDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<DocumentMetadataDto> Metadata { get; set; }
}