namespace DocumentManagementSystem.API.Interfaces;
public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file);
    Task DeleteFileAsync(string filePath);
}
