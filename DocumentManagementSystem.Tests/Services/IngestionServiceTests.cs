using DocumentManagementSystem.API.Services;
using DocumentManagementSystem.Core.Dtos.IngestionDtos;
using DocumentManagementSystem.Core.Entities;
using DocumentManagementSystem.Core.Enums;
using DocumentManagementSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Linq.Expressions;

namespace DocumentManagementSystem.Tests.Services;

public class IngestionServiceTests
{
	private readonly Mock<IUnitOfWork> _mockUnitOfWork;
	private readonly Mock<IConfiguration> _mockConfiguration;
	private readonly HttpClient _httpClient;
	private readonly IngestionService _service;

	public IngestionServiceTests()
	{
		_mockUnitOfWork = new Mock<IUnitOfWork>();
		_mockConfiguration = new Mock<IConfiguration>();

		var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
		_httpClient = new HttpClient(httpMessageHandlerMock.Object);

		_service = new IngestionService(_mockUnitOfWork.Object, _httpClient, _mockConfiguration.Object);
	}

	[Fact]
	public async Task TriggerIngestionAsync_DocumentNotFound_ThrowsKeyNotFoundException()
	{
		// Arrange
		var docId = Guid.NewGuid().ToString();

		_mockUnitOfWork.
			Setup(x => x.DocumentRepository.FindAsync(
			It.IsAny<Expression<Func<Document, bool>>>(),
			It.IsAny<Func<IQueryable<Document>, IIncludableQueryable<Document, object>>>()))
			.ReturnsAsync(Enumerable.Empty<Document>());


		// Act & Assert
		var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.TriggerIngestionAsync(docId));
		Assert.Equal("Document not found.", ex.Message);
	}

	[Fact]
	public async Task GetStatusByIdAsync_StatusNotFound_ThrowsKeyNotFoundException()
	{
		// Arrange
		var ingestionId = Guid.NewGuid().ToString();
		_mockUnitOfWork.Setup(u => u.IngestionStatusRepository.GetByIdAsync(ingestionId))
					   .ReturnsAsync((IngestionStatus)null);

		// Act & Assert
		var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetStatusByIdAsync(ingestionId));
		Assert.Equal("Ingestion status not found.", ex.Message);
	}

	[Fact]
	public async Task CancelIngestionAsync_NotFound_ThrowsKeyNotFoundException()
	{
		// Arrange
		var ingestionId = Guid.NewGuid().ToString();
		_mockUnitOfWork.Setup(u => u.IngestionStatusRepository.GetByIdAsync(ingestionId))
					   .ReturnsAsync((IngestionStatus)null);

		// Act & Assert
		var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CancelIngestionAsync(ingestionId));
		Assert.Equal("Ingestion record not found.", ex.Message);
	}

	[Fact]
	public async Task CancelIngestionAsync_InvalidStatus_ThrowsInvalidOperationException()
	{
		// Arrange
		var ingestionId = Guid.NewGuid().ToString();
		var ingestion = new IngestionStatus
		{
			Id = Guid.Parse(ingestionId),
			Status = IngestionStatusType.Completed.ToString()
		};

		_mockUnitOfWork.Setup(u => u.IngestionStatusRepository.GetByIdAsync(ingestionId))
					   .ReturnsAsync(ingestion);

		// Act & Assert
		var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelIngestionAsync(ingestionId));
		Assert.Equal("Only in-progress ingestions can be cancelled.", ex.Message);
	}

	[Fact]
	public async Task GetIngestionStatusesAsync_ReturnsFilteredList()
	{
		// Arrange
		var docId = Guid.NewGuid();
		var ingestionStatuses = new List<IngestionStatus>
		{
			new IngestionStatus
			{
				Id = Guid.NewGuid(),
				DocumentId = docId,
				Status = IngestionStatusType.InProgress.ToString(),
				TriggeredAt = DateTime.UtcNow
			}
		};

		_mockUnitOfWork.Setup(x => x.IngestionStatusRepository.FindAsync(
			It.IsAny<Expression<Func<IngestionStatus, bool>>>(),
			It.IsAny<Func<IQueryable<IngestionStatus>, IIncludableQueryable<IngestionStatus, object>>>()))
			.ReturnsAsync(ingestionStatuses);

		_mockUnitOfWork.
			Setup(x => x.DocumentRepository.FindAsync(
			It.IsAny<Expression<Func<Document, bool>>>(),
			It.IsAny<Func<IQueryable<Document>, IIncludableQueryable<Document, object>>>()))
			.ReturnsAsync(new List<Document> { new Document { Id = docId } });

		var queryParams = new IngestionQueryParams
		{
			PageNumber = 1,
			PageSize = 10
		};

		// Act
		var result = await _service.GetIngestionStatusesAsync(queryParams);

		// Assert
		Assert.Single(result);
	}

	[Fact]
	public async Task GetStatusByIdAsync_ValidId_ReturnsStatus()
	{
		// Arrange
		var id = Guid.NewGuid();
		var ingestion = new IngestionStatus
		{
			Id = id,
			DocumentId = Guid.NewGuid(),
			Status = IngestionStatusType.Completed.ToString(),
			TriggeredAt = DateTime.UtcNow
		};

		_mockUnitOfWork.Setup(u => u.IngestionStatusRepository.GetByIdAsync(id.ToString()))
			.ReturnsAsync(ingestion);

		// Act
		var result = await _service.GetStatusByIdAsync(id.ToString());

		// Assert
		Assert.NotNull(result);
		Assert.Equal(id, result.Id);
	}

	[Fact]
	public async Task CancelIngestionAsync_ValidInProgress_UpdatesStatus()
	{
		// Arrange
		var ingestionId = Guid.NewGuid();
		var ingestion = new IngestionStatus
		{
			Id = ingestionId,
			Status = IngestionStatusType.InProgress.ToString(),
			DocumentId = Guid.NewGuid(),
			TriggeredAt = DateTime.UtcNow
		};

		_mockUnitOfWork.Setup(u => u.IngestionStatusRepository.GetByIdAsync(ingestionId.ToString()))
			.ReturnsAsync(ingestion);

		_mockUnitOfWork.Setup(u => u.CompleteAsync())
			.ReturnsAsync(1);

		// Act
		await _service.CancelIngestionAsync(ingestionId.ToString());

		// Assert
		Assert.Equal(IngestionStatusType.Failed.ToString(), ingestion.Status);
	}

	[Fact]
	public async Task CancelIngestionAsync_NotInProgress_ThrowsInvalidOperation()
	{
		// Arrange
		var ingestionId = Guid.NewGuid();
		var ingestion = new IngestionStatus
		{
			Id = ingestionId,
			Status = IngestionStatusType.Completed.ToString(),
			DocumentId = Guid.NewGuid(),
			TriggeredAt = DateTime.UtcNow
		};

		_mockUnitOfWork.Setup(u => u.IngestionStatusRepository.GetByIdAsync(ingestionId.ToString()))
			.ReturnsAsync(ingestion);

		// Act & Assert
		var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelIngestionAsync(ingestionId.ToString()));
		Assert.Equal("Only in-progress ingestions can be cancelled.", ex.Message);
	}
}
