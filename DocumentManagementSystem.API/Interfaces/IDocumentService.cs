using DocumentManagementSystem.Core.Dtos.DocumentDtos;

namespace DocumentManagementSystem.API.Interfaces;
public interface IDocumentService
{
    Task<DocumentDto> GetDocumentByIdAsync(Guid id);
    Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync(DocumentQueryParams queryParams);
    Task<DocumentDto> UploadDocumentAsync(UploadDocumentDto createDocumentDto, IFormFile file, string userId);
    Task<DocumentDto> UpdateDocumentAsync(Guid id, UpdateDocumentDto updateDocumentDto, IFormFile file, string userId);
    Task DeleteDocumentAsync(string id, string userId);
}
