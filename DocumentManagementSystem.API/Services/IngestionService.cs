using DocumentManagementSystem.API.Interfaces;
using DocumentManagementSystem.Core.Dtos.IngestionDtos;
using DocumentManagementSystem.Core.Entities;
using DocumentManagementSystem.Core.Enums;
using DocumentManagementSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace DocumentManagementSystem.API.Services;

public class IngestionService : IIngestion
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly HttpClient _httpClient;
	private readonly IConfiguration _configuration;

	public IngestionService(IUnitOfWork unitOfWork, HttpClient httpClient, IConfiguration configuration)
	{
		_unitOfWork = unitOfWork;
		_httpClient = httpClient;
		_configuration = configuration;
	}

	public async Task TriggerIngestionAsync(string documentId)
	{
		// Validate document existence
		var document = (await _unitOfWork.DocumentRepository
			.FindAsync(d => d.Id.ToString() == documentId)).FirstOrDefault();

		if (document == null)
		{
			throw new KeyNotFoundException("Document not found.");
		}

		// Create initial ingestion status
		var ingestionStatus = new IngestionStatus
		{
			Id = Guid.NewGuid(),
			DocumentId = document.Id,
			Status = IngestionStatusType.InProgress.ToString(),
			TriggeredAt = DateTime.UtcNow
		};

		await _unitOfWork.IngestionStatusRepository.AddAsync(ingestionStatus);
		await _unitOfWork.CompleteAsync();

		// Prepare HTTP call
		var payload = new { DocumentId = documentId };
		var json = JsonSerializer.Serialize(payload);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var endpoint = _configuration["IngestionSettings:TriggerApi"];
		var response = await _httpClient.PostAsync(endpoint, content);
		var responseContent = await response.Content.ReadAsStringAsync();

		if (!response.IsSuccessStatusCode)
		{
			ingestionStatus.Status = IngestionStatusType.Failed.ToString();
			await _unitOfWork.CompleteAsync();
			throw new Exception($"Ingestion API failed. Response: {responseContent}");
		}

		try
		{
			var responseJson = JsonSerializer.Deserialize<IngestionResponseDto>(responseContent);
			if (responseJson?.Status != null &&
				Enum.TryParse<IngestionStatusType>(responseJson.Status, true, out var newStatus))
			{
				ingestionStatus.Status = newStatus.ToString();
			}
			else
			{
				ingestionStatus.Status = IngestionStatusType.Failed.ToString();
			}

			await _unitOfWork.CompleteAsync();
		}
		catch (Exception ex)
		{
			ingestionStatus.Status = IngestionStatusType.Failed.ToString();
			await _unitOfWork.CompleteAsync();
			throw new Exception($"Failed to parse ingestion response. Error: {ex.Message}");
		}
	}

	public async Task<IEnumerable<IngestionStatusDto>> GetIngestionStatusesAsync(IngestionQueryParams queryParams)
	{
		var ingestionStatuses = await _unitOfWork.IngestionStatusRepository.FindAsync(
			predicate: i =>
				(string.IsNullOrEmpty(queryParams.DocumentId) || i.DocumentId.ToString() == queryParams.DocumentId) &&
				(string.IsNullOrEmpty(queryParams.Status) || i.Status == queryParams.Status) &&
				(!queryParams.FromDate.HasValue || i.TriggeredAt >= queryParams.FromDate.Value) &&
				(!queryParams.ToDate.HasValue || i.TriggeredAt <= queryParams.ToDate.Value)
		);

		IQueryable<IngestionStatus> query = ingestionStatuses.AsQueryable();

		if (!string.IsNullOrWhiteSpace(queryParams.SortBy))
		{
			var sortBy = queryParams.SortBy.Trim();

			if (sortBy.Equals(nameof(IngestionStatus.TriggeredAt), StringComparison.OrdinalIgnoreCase))
			{
				query = queryParams.SortDescending
					? query.OrderByDescending(i => i.TriggeredAt)
					: query.OrderBy(i => i.TriggeredAt);
			}
			else if (sortBy.Equals(nameof(IngestionStatus.Status), StringComparison.OrdinalIgnoreCase))
			{
				query = queryParams.SortDescending
					? query.OrderByDescending(i => i.Status)
					: query.OrderBy(i => i.Status);
			}
			else
			{
				query = query.OrderByDescending(i => i.TriggeredAt);
			}
		}
		else
		{
			query = query.OrderByDescending(i => i.TriggeredAt);
		}

		var paginated = query
			.Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
			.Take(queryParams.PageSize)
			.ToList();

		var documentIds = paginated.Select(i => i.DocumentId).Distinct().ToList();
		var documents = await _unitOfWork.DocumentRepository.FindAsync(d => documentIds.Contains(d.Id));
		var documentDict = documents.ToDictionary(d => d.Id, d => d);

		var result = paginated.Select(i => new IngestionStatusDto
		{
			Id = i.Id,
			DocumentId = i.DocumentId,
			Status = Enum.Parse<IngestionStatusType>(i.Status, ignoreCase: true),
			TriggeredAt = i.TriggeredAt,
		});

		return result;
	}


	public async Task<IngestionStatusDto> GetStatusByIdAsync(string id)
	{
		var ingestion = await _unitOfWork.IngestionStatusRepository.GetByIdAsync(id);

		if (ingestion == null)
		{
			throw new KeyNotFoundException("Ingestion status not found.");
		}

		return new IngestionStatusDto
		{
			Id = ingestion.Id,
			DocumentId = ingestion.DocumentId,
			TriggeredAt = ingestion.TriggeredAt,
			Status = Enum.Parse<IngestionStatusType>(ingestion.Status)
		};
	}

	public async Task CancelIngestionAsync(string ingestionId)
	{
		var ingestion = await _unitOfWork.IngestionStatusRepository.GetByIdAsync(ingestionId);

		if (ingestion == null)
		{
			throw new KeyNotFoundException("Ingestion record not found.");
		}

		if (ingestion.Status != IngestionStatusType.InProgress.ToString())
		{
			throw new InvalidOperationException("Only in-progress ingestions can be cancelled.");
		}

		// Mark as failed (cancelled in our case)
		ingestion.Status = IngestionStatusType.Failed.ToString();

		await _unitOfWork.CompleteAsync();
	}
}
