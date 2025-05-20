using System.Text.Json.Serialization;

public class IngestionResponseDto
{
	[JsonPropertyName("status")]
	public string Status { get; set; }

	[JsonPropertyName("message")]
	public string Message { get; set; }
}
