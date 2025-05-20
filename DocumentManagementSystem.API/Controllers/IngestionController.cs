using DocumentManagementSystem.Core.Dtos.IngestionDtos;
using DocumentManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IngestionController : ControllerBase
{
	private readonly IIngestion _ingestionService;

	public IngestionController(IIngestion ingestionService)
	{
		_ingestionService = ingestionService;
	}

	// POST api/ingestion/trigger/{documentId}
	[HttpPost("trigger/{documentId}")]
	public async Task<IActionResult> TriggerIngestion(string documentId)
	{
		if (string.IsNullOrWhiteSpace(documentId))
			return BadRequest("DocumentId must be provided.");

		try
		{
			await _ingestionService.TriggerIngestionAsync(documentId);
			return Ok(new { Message = "Ingestion triggered successfully." });
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(new { Message = ex.Message });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Message = "An error occurred while triggering ingestion.", Details = ex.Message });
		}
	}

	[HttpGet("statuses")]
	public async Task<IActionResult> GetIngestionStatuses([FromQuery] IngestionQueryParams queryParams)
	{
		try
		{
			var statuses = await _ingestionService.GetIngestionStatusesAsync(queryParams);
			return Ok(statuses);
		}
		catch (Exception ex)
		{
			// Log exception as needed
			return StatusCode(500, new { Message = "An error occurred while fetching ingestion statuses.", Details = ex.Message });
		}
	}

	[HttpGet("status/{id}")]
	public async Task<IActionResult> GetStatusById(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
			return BadRequest("Ingestion status ID must be provided.");

		try
		{
			var status = await _ingestionService.GetStatusByIdAsync(id);
			return Ok(status);
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(new { Message = ex.Message });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Message = "An error occurred while fetching ingestion status.", Details = ex.Message });
		}
	}

	[HttpPost("cancel/{ingestionId}")]
	public async Task<IActionResult> CancelIngestion(string ingestionId)
	{
		if (string.IsNullOrWhiteSpace(ingestionId))
			return BadRequest("Ingestion ID must be provided.");

		try
		{
			await _ingestionService.CancelIngestionAsync(ingestionId);
			return Ok(new { Message = "Ingestion cancelled successfully." });
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(new { Message = ex.Message });
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(new { Message = ex.Message });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Message = "An error occurred while cancelling ingestion.", Details = ex.Message });
		}
	}
}
