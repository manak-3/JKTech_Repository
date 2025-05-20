using DocumentManagementSystem.API.Controllers;
using DocumentManagementSystem.Core.Dtos.IngestionDtos;
using DocumentManagementSystem.Core.Enums;
using DocumentManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class IngestionControllerTests
{
	private readonly Mock<IIngestion> _mockIngestionService;
	private readonly IngestionController _controller;

	public IngestionControllerTests()
	{
		_mockIngestionService = new Mock<IIngestion>();
		_controller = new IngestionController(_mockIngestionService.Object);
	}

	[Fact]
	public async Task TriggerIngestionAsync_ValidDocumentId_ReturnsOk()
	{
		// Arrange
		var docId = Guid.NewGuid().ToString();

		_mockIngestionService
			.Setup(s => s.TriggerIngestionAsync(docId))
			.Returns(Task.CompletedTask);

		// Act
		var result = await _controller.TriggerIngestion(docId);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
	}

	[Fact]
	public async Task TriggerIngestionAsync_DocumentNotFound_ReturnsNotFound()
	{
		var docId = Guid.NewGuid().ToString();

		_mockIngestionService
			.Setup(s => s.TriggerIngestionAsync(docId))
			.ThrowsAsync(new KeyNotFoundException("Document not found."));

		var result = await _controller.TriggerIngestion(docId);
		var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
		Assert.Equal(404, notFoundResult.StatusCode);
	}

	[Fact]
	public async Task GetIngestionStatusesAsync_ValidQueryParams_ReturnsPagedResults()
	{
		var queryParams = new IngestionQueryParams
		{
			PageNumber = 1,
			PageSize = 2,
			SortBy = "TriggeredAt",
			SortDescending = true
		};

		var sampleStatuses = new List<IngestionStatusDto>
		{
			new IngestionStatusDto
			{
				Id = Guid.NewGuid(),
				DocumentId = Guid.NewGuid(),
				Status = IngestionStatusType.Completed,
				TriggeredAt = DateTime.UtcNow.AddDays(-1)
			},
			new IngestionStatusDto
			{
				Id = Guid.NewGuid(),
				DocumentId = Guid.NewGuid(),
				Status = IngestionStatusType.InProgress,
				TriggeredAt = DateTime.UtcNow
			}
		};

		_mockIngestionService
			.Setup(s => s.GetIngestionStatusesAsync(queryParams))
			.ReturnsAsync(sampleStatuses);

		var result = await _controller.GetIngestionStatuses(queryParams);

		var okResult = Assert.IsType<OkObjectResult>(result);
		var returnStatuses = Assert.IsAssignableFrom<IEnumerable<IngestionStatusDto>>(okResult.Value);
		Assert.Equal(2, ((List<IngestionStatusDto>)returnStatuses).Count);
	}

	[Fact]
	public async Task GetStatusByIdAsync_NotFound_ReturnsNotFound()
	{
		var id = Guid.NewGuid().ToString();

		_mockIngestionService
			.Setup(s => s.GetStatusByIdAsync(id))
			.ThrowsAsync(new KeyNotFoundException("Ingestion status not found."));

		var result = await _controller.GetStatusById(id);

		var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
		Assert.Equal(404, notFoundResult.StatusCode);

	}

	[Fact]
	public async Task GetStatusByIdAsync_Found_ReturnsStatus()
	{
		var id = Guid.NewGuid().ToString();
		var ingestionStatus = new IngestionStatusDto
		{
			Id = Guid.Parse(id),
			DocumentId = Guid.NewGuid(),
			Status = IngestionStatusType.Completed,
			TriggeredAt = DateTime.UtcNow
		};

		_mockIngestionService
			.Setup(s => s.GetStatusByIdAsync(id))
			.ReturnsAsync(ingestionStatus);

		var result = await _controller.GetStatusById(id);

		var okResult = Assert.IsType<OkObjectResult>(result);
		var returnedStatus = Assert.IsType<IngestionStatusDto>(okResult.Value);
		Assert.Equal(ingestionStatus.Id, returnedStatus.Id);
	}

	[Fact]
	public async Task CancelIngestionAsync_NotFound_ReturnsNotFound()
	{
		var id = Guid.NewGuid().ToString();

		_mockIngestionService
			.Setup(s => s.CancelIngestionAsync(id))
			.ThrowsAsync(new KeyNotFoundException("Ingestion record not found."));

		var result = await _controller.CancelIngestion(id);

		var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
		var messageProperty = notFoundResult.Value.GetType().GetProperty("Message");
		Assert.NotNull(messageProperty);

		var message = messageProperty.GetValue(notFoundResult.Value) as string;
		Assert.Equal("Ingestion record not found.", message);
	}
}

