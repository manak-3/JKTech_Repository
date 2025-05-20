namespace DocumentManagementSystem.Core.Dtos.DocumentDtos;
public class DocumentQueryParams
{
    public string FileName { get; set; }
    public string FileType { get; set; }
    public string Description { get; set; }
    public DateTime? FromDate { get; set; }

    public string Category { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "UploadDate";
    public bool SortDescending { get; set; } = true;
}