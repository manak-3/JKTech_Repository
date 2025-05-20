using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using DocumentManagementSystem.API.Controllers;
using DocumentManagementSystem.API.Interfaces;
using DocumentManagementSystem.Core.Dtos.DocumentDtos;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using FluentAssertions;
using System.Linq.Expressions;

public class DocumentsControllerTests
{
	private readonly Mock<IDocumentService> _documentServiceMock;
	private readonly DocumentsController _controller;

	public DocumentsControllerTests()
	{
		_documentServiceMock = new Mock<IDocumentService>();
		_controller = new DocumentsController(_documentServiceMock.Object);
	}

	private void SetUser(string userId)
	{
		var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId)
		}));
		_controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext { User = user }
		};
	}

	[Fact]
	public async Task GetDocuments_ShouldReturnOkWithDocuments()
	{
		var expected = new List<DocumentDto> { new DocumentDto { Id = Guid.NewGuid(), Name = "Doc1" } };
		_documentServiceMock.Setup(s => s.GetAllDocumentsAsync(It.IsAny<DocumentQueryParams>()))
							.ReturnsAsync(expected);

		var result = await _controller.GetDocuments(new DocumentQueryParams());

		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		okResult.Value.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public async Task GetDocument_ShouldReturnOk()
	{
		var docId = Guid.NewGuid();
		var doc = new DocumentDto { Id = docId, Name = "Sample" };
		_documentServiceMock.Setup(s => s.GetDocumentByIdAsync(docId)).ReturnsAsync(doc);

		var result = await _controller.GetDocument(docId);

		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		okResult.Value.Should().Be(doc);
	}

	[Fact]
	public async Task UploadDocument_ShouldReturnCreated()
	{
		var userId = "user-123";
		SetUser(userId);

		var fileMock = new Mock<IFormFile>();
		var request = new DocumentRequestDto
		{
			Name = "Doc",
			Description = "Test",
			Metadata = "[]",
			File = fileMock.Object
		};
		var uploadedDoc = new DocumentDto { Id = Guid.NewGuid(), Name = "Doc" };

		_documentServiceMock.Setup(s => s.UploadDocumentAsync(
			It.IsAny<UploadDocumentDto>(), It.IsAny<IFormFile>(), userId))
			.ReturnsAsync(uploadedDoc);

		var result = await _controller.UploadDocument(request);

		var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
		createdAt.Value.Should().Be(uploadedDoc);
	}

	[Fact]
	public async Task UploadDocument_Unauthorized_IfNoUser()
	{
		string userId = string.Empty;
		SetUser(userId);
		var result = await _controller.UploadDocument(new DocumentRequestDto());
		Assert.IsType<UnauthorizedResult>(result.Result);
	}

	[Fact]
	public async Task UpdateDocument_ShouldReturnOk()
	{
		var userId = "user-123";
		SetUser(userId);

		var fileMock = new Mock<IFormFile>();
		var request = new DocumentRequestDto
		{
			Name = "Doc",
			Description = "Updated",
			Metadata = "[]",
			File = fileMock.Object
		};
		var updatedDoc = new DocumentDto { Id = Guid.NewGuid(), Name = "Updated Doc" };

		_documentServiceMock.Setup(s => s.UpdateDocumentAsync(
			It.IsAny<Guid>(), It.IsAny<UpdateDocumentDto>(), It.IsAny<IFormFile>(), userId))
			.ReturnsAsync(updatedDoc);

		var result = await _controller.UpdateDocument(Guid.NewGuid(), request);

		var okResult = Assert.IsType<OkObjectResult>(result);
		okResult.Value.Should().Be(updatedDoc);
	}

	[Fact]
	public async Task UpdateDocument_ShouldReturnNotFound_IfKeyMissing()
	{
		var userId = "user-123";
		SetUser(userId);

		var request = new DocumentRequestDto { Metadata = "[]" };

		_documentServiceMock.Setup(s => s.UpdateDocumentAsync(
			It.IsAny<Guid>(), It.IsAny<UpdateDocumentDto>(), It.IsAny<IFormFile>(), userId))
			.ThrowsAsync(new KeyNotFoundException("Document not found"));

		var result = await _controller.UpdateDocument(Guid.NewGuid(), request);

		var notFound = Assert.IsType<NotFoundObjectResult>(result);
		Assert.Equal("Document not found", notFound.Value);
	}

	[Fact]
	public async Task UpdateDocument_ShouldReturnForbidden_IfNotAuthorized()
	{
		var userId = "user-123";
		SetUser(userId);

		var request = new DocumentRequestDto { Metadata = "[]" };

		_documentServiceMock.Setup(s => s.UpdateDocumentAsync(
			It.IsAny<Guid>(), It.IsAny<UpdateDocumentDto>(), It.IsAny<IFormFile>(), userId))
			.ThrowsAsync(new UnauthorizedAccessException("Access denied"));

		var result = await _controller.UpdateDocument(Guid.NewGuid(), request);

		var forbid = Assert.IsType<ForbidResult>(result);
	}

	[Fact]
	public async Task DeleteDocument_ShouldReturnNoContent()
	{
		var userId = "user-123";
		SetUser(userId);

		_documentServiceMock.Setup(s => s.DeleteDocumentAsync(It.IsAny<string>(), userId))
			.Returns(Task.CompletedTask);

		var result = await _controller.DeleteDocument(Guid.NewGuid().ToString());

		Assert.IsType<NoContentResult>(result);
	}

	[Fact]
	public async Task DeleteDocument_Unauthorized_IfNoUser()
	{
		string userId = string.Empty;
		SetUser(userId);
		var result = await _controller.DeleteDocument(Guid.NewGuid().ToString());
		Assert.IsType<UnauthorizedResult>(result);
	}

	[Fact]
	public async Task DeleteDocument_ShouldReturnNotFound()
	{
		var userId = "user-123";
		SetUser(userId);

		_documentServiceMock.Setup(s => s.DeleteDocumentAsync(It.IsAny<string>(), userId))
			.ThrowsAsync(new KeyNotFoundException("Not found"));

		var result = await _controller.DeleteDocument(Guid.NewGuid().ToString());

		var notFound = Assert.IsType<NotFoundObjectResult>(result);
		Assert.Equal("Not found", notFound.Value);
	}

	[Fact]
	public async Task DeleteDocument_ShouldReturnForbidden()
	{
		var userId = "user-123";
		SetUser(userId);

		_documentServiceMock.Setup(s => s.DeleteDocumentAsync(It.IsAny<string>(), userId))
			.ThrowsAsync(new UnauthorizedAccessException("Not allowed"));

		var result = await _controller.DeleteDocument(Guid.NewGuid().ToString());

		Assert.IsType<ForbidResult>(result);
	}
}
