namespace DocumentManagementSystem.Core.Entities;

public class IngestionStatus
{
	public Guid Id { get; set; }
	public Guid DocumentId { get; set; }
	public string Status { get; set; } // InProgress, Completed, Failed
	public DateTime TriggeredAt { get; set; }
}
