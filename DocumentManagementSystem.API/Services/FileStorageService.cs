using DocumentManagementSystem.API.Interfaces;

namespace DocumentManagementSystem.API.Services;

public class FileStorageService : IFileStorageService
{
	private readonly string _uploadsFolderPath;

	public FileStorageService()
	{
		// Define the uploads folder path relative to the app base directory
		_uploadsFolderPath = Path.Combine(AppContext.BaseDirectory, "Uploads");

		// Ensure the uploads directory exists
		if (!Directory.Exists(_uploadsFolderPath))
		{
			Directory.CreateDirectory(_uploadsFolderPath);
		}
	}

	public async Task<string> SaveFileAsync(IFormFile file)
	{
		if (file == null || file.Length == 0)
		{
			throw new ArgumentException("File is empty");
		}

		var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
		var fullFilePath = Path.Combine(_uploadsFolderPath, fileName);

		using (var stream = new FileStream(fullFilePath, FileMode.Create))
		{
			await file.CopyToAsync(stream);
		}
		return fileName;
	}

	public async Task DeleteFileAsync(string savedFileName)
	{
		if (string.IsNullOrEmpty(savedFileName))
				return;

			var fullPath = Path.Combine(_uploadsFolderPath, savedFileName);

			if (File.Exists(fullPath))
			{
				await Task.Run(() => File.Delete(fullPath));
			}
		}
	}
