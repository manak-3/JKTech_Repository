namespace DocumentManagementSystem.Core.Dtos.IngestionDtos;

public class IngestionQueryParams
{
	public string DocumentId { get; set; }
	public string Status { get; set; } // InProgress, Completed, Failed
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
	public int PageNumber { get; set; } = 1;
	public int PageSize { get; set; } = 10;
	public string SortBy { get; set; } = "TriggeredAt";
	public bool SortDescending { get; set; } = true;
}
