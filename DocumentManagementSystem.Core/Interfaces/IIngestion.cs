using DocumentManagementSystem.Core.Dtos.IngestionDtos;

namespace DocumentManagementSystem.Core.Interfaces;

public interface IIngestion
{
	Task TriggerIngestionAsync(string documentId);
	Task<IEnumerable<IngestionStatusDto>> GetIngestionStatusesAsync(IngestionQueryParams queryParams);
	Task<IngestionStatusDto> GetStatusByIdAsync(string id);
	Task CancelIngestionAsync(string ingestionId);
}