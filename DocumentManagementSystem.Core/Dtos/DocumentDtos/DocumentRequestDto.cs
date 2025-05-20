using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagementSystem.Core.Dtos.DocumentDtos;
public class DocumentRequestDto
{
	public string Name { get; set; }
	public string Description { get; set; }
	public IFormFile File { get; set; }

	[FromForm(Name = "metadata")]
	public string Metadata { get; set; }
}