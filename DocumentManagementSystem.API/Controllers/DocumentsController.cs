using DocumentManagementSystem.API.Interfaces;
using DocumentManagementSystem.Core.Dtos.DocumentDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace DocumentManagementSystem.API.Controllers;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

	[HttpGet]
	public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocuments([FromQuery] DocumentQueryParams queryParams)
	{
		var documents = await _documentService.GetAllDocumentsAsync(queryParams);
		return Ok(documents);
	}

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetDocument(Guid id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            return Ok(document);
        }

	[HttpPost]
	public async Task<ActionResult<DocumentDto>> UploadDocument([FromForm] DocumentRequestDto request)
	{
		var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (string.IsNullOrEmpty(userId))
			return Unauthorized();

		var metadata = string.IsNullOrEmpty(request.Metadata)
			? new List<DocumentMetadataDto>()
			: JsonConvert.DeserializeObject<List<DocumentMetadataDto>>(request.Metadata);

		var createDto = new UploadDocumentDto
		{
			Name = request.Name,
			Description = request.Description,
			Metadata = metadata
		};

		var document = await _documentService.UploadDocumentAsync(createDto, request.File, userId);
		return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateDocument(Guid id, [FromForm] DocumentRequestDto request)
	{
		var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (string.IsNullOrEmpty(userId))
		{
			return Unauthorized();
		}

		var metadata = string.IsNullOrEmpty(request.Metadata)
			? new List<DocumentMetadataDto>()
			: JsonConvert.DeserializeObject<List<DocumentMetadataDto>>(request.Metadata);

		var updateDto = new UpdateDocumentDto
		{
			Name = request.Name,
			Description = request.Description,
			Metadata = metadata
		};

		try
		{
			var document = await _documentService.UpdateDocumentAsync(id, updateDto, request.File, userId);
			return Ok(document);
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(ex.Message);
		}
		catch (UnauthorizedAccessException ex)
		{
			return Forbid(ex.Message);
		}
	}


	[HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                await _documentService.DeleteDocumentAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }