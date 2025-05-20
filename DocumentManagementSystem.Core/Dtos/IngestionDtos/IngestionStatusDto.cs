using DocumentManagementSystem.Core.Enums;

public class IngestionStatusDto
{
	public Guid Id { get; set; }
	public Guid DocumentId { get; set; }
	public IngestionStatusType Status { get; set; }
	public DateTime TriggeredAt { get; set; }
}
