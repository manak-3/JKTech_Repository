using AutoMapper;
using DocumentManagementSystem.API.Interfaces;
using DocumentManagementSystem.Core.Dtos.DocumentDtos;
using DocumentManagementSystem.Core.Entities;
using DocumentManagementSystem.Core.Enums;
using DocumentManagementSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementSystem.API.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;

    public DocumentService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
    }

    public async Task<DocumentDto> GetDocumentByIdAsync(Guid id)
    {
        var document = await _unitOfWork.DocumentRepository.FindAsync(d => d.Id == id,
            include: query => query.Include(d => d.UploadedByUser)
                                   .Include(d => d.Metadata));

        if (document == null || !document.Any())
        {
            throw new KeyNotFoundException("Document not found");
        }

        var documentDto = _mapper.Map<DocumentDto>(document.First());
        documentDto.UploadedByUserName = $"{document.First().UploadedByUser.FirstName} {document.First().UploadedByUser.LastName}";
        return documentDto;
    }

    public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync(DocumentQueryParams queryParams)
    {
        var query = _unitOfWork.DocumentRepository.GetQueryable()
            .Include(d => d.UploadedByUser)
            .Include(d => d.Metadata)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(queryParams.FileName))
        {
            query = query.Where(d => 
                d.Name.Contains(queryParams.FileName) || 
                d.Description.Contains(queryParams.Description));
        }

        if (!string.IsNullOrEmpty(queryParams.FileType))
        {
            query = query.Where(d => 
                d.Metadata.Any(m => m.Key == "FileType" && m.Value.Contains(queryParams.FileType)));
        }

        if (!string.IsNullOrEmpty(queryParams.Category))
        {
            query = query.Where(d => 
                d.Metadata.Any(m => m.Key == "Category" && m.Value.Contains(queryParams.Category)));
        }

        if (queryParams.FromDate.HasValue)
        {
            query = query.Where(d => d.UploadDate >= queryParams.FromDate.Value);
        }

        if (queryParams.ToDate.HasValue)
        {
            query = query.Where(d => d.UploadDate <= queryParams.ToDate.Value);
        }

        // Apply sorting
        query = queryParams.SortBy.ToLower() switch
        {
            "name" => queryParams.SortDescending ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
            "uploaddate" => queryParams.SortDescending ? query.OrderByDescending(d => d.UploadDate) : query.OrderBy(d => d.UploadDate),
            "filesize" => queryParams.SortDescending ? query.OrderByDescending(d => d.FileSize) : query.OrderBy(d => d.FileSize),
            _ => queryParams.SortDescending ? query.OrderByDescending(d => d.UploadDate) : query.OrderBy(d => d.UploadDate)
        };

        // Apply pagination
        query = query.Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize);

        var documents = await query.ToListAsync();
        var documentDtos = _mapper.Map<IEnumerable<DocumentDto>>(documents);

        // Map user names
        foreach (var docDto in documentDtos)
        {
            var doc = documents.First(d => d.Id == docDto.Id);
            docDto.UploadedByUserName = $"{doc.UploadedByUser.FirstName} {doc.UploadedByUser.LastName}";
        }

        return documentDtos;
    }

	public async Task<DocumentDto> UploadDocumentAsync(UploadDocumentDto createDocumentDto, IFormFile file, string userId)
	{
		if (file == null || file.Length == 0)
		{
			throw new ArgumentException("File is empty");
		}

		if (file.Length > 10 * 1024 * 1024)
		{
			throw new ArgumentException("File size exceeds the maximum limit of 10MB");
		}

		var filePath = await _fileStorageService.SaveFileAsync(file);

		var document = new Document
		{
			Name = createDocumentDto.Name,
			Description = createDocumentDto.Description,
			FilePath = filePath,
			ContentType = file.ContentType,
			FileSize = file.Length,
			UploadedByUserId = userId,
			Metadata = _mapper.Map<List<DocumentMetadata>>(createDocumentDto.Metadata)
		};

		await _unitOfWork.DocumentRepository.AddAsync(document);
		await _unitOfWork.CompleteAsync();

		return _mapper.Map<DocumentDto>(document);
	}


	public async Task<DocumentDto> UpdateDocumentAsync(Guid id, UpdateDocumentDto updateDocumentDto, IFormFile file, string userId)
    {
        var document = (await _unitOfWork.DocumentRepository.FindAsync(d => d.Id == id, 
            include: query => query.Include(d => d.Metadata))).FirstOrDefault();

        if (document == null)
        {
            throw new KeyNotFoundException("Document not found");
        }

        // Check if user has permission to update (owner or admin)
        if (document.UploadedByUserId != userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            var isAdmin = await _unitOfWork.UserManager.IsInRoleAsync(user, RoleType.Admin.ToString());
            if (!isAdmin)
            {
                throw new UnauthorizedAccessException("You don't have permission to update this document");
            }
        }

        // Update document properties
        document.Name = updateDocumentDto.Name ?? document.Name;
        document.Description = updateDocumentDto.Description ?? document.Description;
        document.LastModified = DateTime.UtcNow;

        // Update metadata
        if (updateDocumentDto.Metadata != null && updateDocumentDto.Metadata.Any())
        {
            // Remove existing metadata
            var existingMetadata = document.Metadata.ToList();
            foreach (var meta in existingMetadata)
            {
                _unitOfWork.DocumentMetadataRepository.Remove(meta);
            }

            // Add new metadata
            document.Metadata = _mapper.Map<List<DocumentMetadata>>(updateDocumentDto.Metadata);
        }

        // Update file if provided
        if (file != null && file.Length > 0)
        {
            // Delete old file
            await _fileStorageService.DeleteFileAsync(document.FilePath);

            // Save new file
            document.FilePath = await _fileStorageService.SaveFileAsync(file);
            document.ContentType = file.ContentType;
            document.FileSize = file.Length;
        }

        _unitOfWork.DocumentRepository.Update(document);
        await _unitOfWork.CompleteAsync();

        return _mapper.Map<DocumentDto>(document);
    }

    public async Task DeleteDocumentAsync(string id, string userId)
    {
		Guid documentId;
		if (!Guid.TryParse(id, out documentId))
		{
			throw new ArgumentException("Invalid document ID");
		}

		var document = await _unitOfWork.DocumentRepository.GetByIdAsync(id);
        if (document == null)
        {
            throw new KeyNotFoundException("Document not found");
        }

        // Check if user has permission to delete (owner or admin)
        if (document.UploadedByUserId != userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            var isAdmin = await _unitOfWork.UserManager.IsInRoleAsync(user, RoleType.Admin.ToString());
            if (!isAdmin)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this document");
            }
        }

        // Delete file from storage
        await _fileStorageService.DeleteFileAsync(document.FilePath);

        // Delete document
        _unitOfWork.DocumentRepository.Remove(document);
        await _unitOfWork.CompleteAsync();
    }
}