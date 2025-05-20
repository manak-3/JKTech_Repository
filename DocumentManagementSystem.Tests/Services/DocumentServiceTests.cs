using AutoMapper;
using DocumentManagementSystem.API.Interfaces;
using DocumentManagementSystem.API.Services;
using DocumentManagementSystem.Core.Dtos.DocumentDtos;
using DocumentManagementSystem.Core.Entities;
using DocumentManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;

namespace DocumentManagementSystem.Tests.Services;

public class DocumentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly DocumentService _documentService;
    private readonly Mock<UserManager<User>> _userManagerMock;

    public DocumentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _userManagerMock = GetMockUserManager();

        _unitOfWorkMock.Setup(x => x.UserManager).Returns(_userManagerMock.Object);

        _documentService = new DocumentService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _fileStorageServiceMock.Object
        );
    }
	[Fact]
	public async Task GetDocumentByIdAsync_ReturnsDocument_WhenFound()
	{
		// Arrange
		var documentId = Guid.NewGuid();
		var user = new User { Id = "user123", FirstName = "Jane", LastName = "Doe" };

		var document = new Document
		{
			Id = documentId,
			Name = "Test Document",
			UploadedByUserId = user.Id,
			UploadedByUser = user,
			Metadata = new List<DocumentMetadata>()
		};

		var mockDocumentRepo = new Mock<IRepository<Document>>();
		mockDocumentRepo
	.Setup(repo => repo.FindAsync(
		It.Is<Expression<Func<Document, bool>>>(expr => TestDocumentFilter(expr, documentId)),
		It.IsAny<Func<IQueryable<Document>, IQueryable<Document>>>()))
	.ReturnsAsync(new List<Document> { document });

		var mockUnitOfWork = new Mock<IUnitOfWork>();
		mockUnitOfWork.Setup(uow => uow.DocumentRepository).Returns(mockDocumentRepo.Object);

		var mapperMock = new Mock<IMapper>();
		mapperMock.Setup(m => m.Map<DocumentDto>(It.IsAny<Document>()))
			.Returns((Document doc) => new DocumentDto
			{
				Id = doc.Id,
				Name = doc.Name
			});

		var fileStorageServiceMock = new Mock<IFileStorageService>();

		var documentService = new DocumentService(mockUnitOfWork.Object, mapperMock.Object, fileStorageServiceMock.Object);

		// Act
		var result = await documentService.GetDocumentByIdAsync(documentId);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(documentId, result.Id);
		Assert.Equal("Test Document", result.Name);
	}

	[Fact]
    public async Task GetDocumentByIdAsync_ThrowsKeyNotFound_WhenNotFound()
    {
        _unitOfWorkMock.Setup(x => x.DocumentRepository.FindAsync(
            It.IsAny<Expression<Func<Document, bool>>>(),
            It.IsAny<Func<IQueryable<Document>, IIncludableQueryable<Document, object>>>()))
            .ReturnsAsync(new List<Document>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _documentService.GetDocumentByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UploadDocumentAsync_UploadsSuccessfully()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        _fileStorageServiceMock.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync("path/to/file");

        _unitOfWorkMock.Setup(u => u.DocumentRepository.AddAsync(It.IsAny<Document>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        _mapperMock.Setup(m => m.Map<DocumentDto>(It.IsAny<Document>())).Returns(new DocumentDto { Id = Guid.NewGuid() });

        var result = await _documentService.UploadDocumentAsync(
            new UploadDocumentDto { Name = "Test", Description = "Desc" }, fileMock.Object, "userId");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task UploadDocumentAsync_Throws_WhenFileIsEmpty()
    {
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(0);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _documentService.UploadDocumentAsync(new UploadDocumentDto(), file.Object, "userId"));
    }

    [Fact]
    public async Task UploadDocumentAsync_Throws_WhenFileTooLarge()
    {
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(11 * 1024 * 1024); // >10MB

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _documentService.UploadDocumentAsync(new UploadDocumentDto(), file.Object, "userId"));
    }

    [Fact]
    public async Task UpdateDocumentAsync_Throws_WhenDocumentNotFound()
    {
        _unitOfWorkMock.Setup(x => x.DocumentRepository.FindAsync(It.IsAny<Expression<Func<Document, bool>>>(), It.IsAny<Func<IQueryable<Document>, IIncludableQueryable<Document, object>>>()))
                       .ReturnsAsync(new List<Document>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _documentService.UpdateDocumentAsync(Guid.NewGuid(), new UpdateDocumentDto(), null, "userId"));
    }

    [Fact]
    public async Task UpdateDocumentAsync_Throws_WhenUnauthorized()
    {
        var documentId = Guid.NewGuid();
        var doc = new Document { Id = documentId, UploadedByUserId = "anotherUser", FilePath = "file/path" };

        _unitOfWorkMock.Setup(x => x.DocumentRepository.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(doc);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync("userId")).ReturnsAsync(new User());

        _userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<User>(), "Admin")).ReturnsAsync(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _documentService.DeleteDocumentAsync(documentId.ToString(), "userId"));
    }

    [Fact]
    public async Task DeleteDocumentAsync_Deletes_WhenAuthorized()
    {
        var doc = new Document { Id = Guid.NewGuid(), UploadedByUserId = "userId", FilePath = "file/path" };
        _unitOfWorkMock.Setup(x => x.DocumentRepository.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(doc);
        _fileStorageServiceMock.Setup(f => f.DeleteFileAsync(doc.FilePath)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        await _documentService.DeleteDocumentAsync(doc.Id.ToString(), "userId");
        _unitOfWorkMock.Verify(x => x.DocumentRepository.Remove(doc), Times.Once);
    }

    [Fact]
    public async Task DeleteDocumentAsync_Throws_WhenUnauthorized()
    {
        var doc = new Document { Id = Guid.NewGuid(), UploadedByUserId = "anotherUser", FilePath = "file/path" };
        _unitOfWorkMock.Setup(x => x.DocumentRepository.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(doc);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new User());
        _unitOfWorkMock.Setup(x => x.UserManager.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _documentService.DeleteDocumentAsync(doc.Id.ToString(), "userId"));
    }

    [Fact]
    public async Task DeleteDocumentAsync_Throws_WhenIdIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _documentService.DeleteDocumentAsync("invalid-guid", "userId"));
    }

    private static Mock<UserManager<User>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(
            store.Object,
            null,
            null,
            new IUserValidator<User>[0],
            new IPasswordValidator<User>[0],
            null,
            null,
            null,
            null
        );
    }

	private bool TestDocumentFilter(Expression<Func<Document, bool>> expr, Guid expectedId)
	{
		var testDoc = new Document { Id = expectedId };
		var func = expr.Compile();
		return func(testDoc);
	}
}