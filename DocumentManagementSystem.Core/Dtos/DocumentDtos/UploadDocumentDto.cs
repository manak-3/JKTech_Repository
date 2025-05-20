using Microsoft.AspNetCore.Http;

namespace DocumentManagementSystem.Core.Dtos.DocumentDtos;
public class UploadDocumentDto
{
	public string Name { get; set; }
	public string Description { get; set; }
	public IFormFile File { get; set; }
	public List<DocumentMetadataDto> Metadata { get; set; }
}