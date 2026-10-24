using System.ComponentModel.DataAnnotations;

namespace FarmaApi.DTOs.Documents;

public class CreateDocumentDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = "land_record";

    [Required]
    [MaxLength(100)]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string IssuingAuthority { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string IssueDate { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ExpiryDate { get; set; }

    public decimal? LandAreaAcres { get; set; }

    [MaxLength(100)]
    public string? PlotNumber { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string FileSize { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FileType { get; set; } = "application/pdf";

    public string? FileData { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class DocumentResponseDto
{
    public Guid Id { get; set; }
    public Guid FarmerId { get; set; }
    public string FarmerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string IssuingAuthority { get; set; } = string.Empty;
    public string IssueDate { get; set; } = string.Empty;
    public string? ExpiryDate { get; set; }
    public decimal? LandAreaAcres { get; set; }
    public string? PlotNumber { get; set; }
    public string Status { get; set; } = "verified";
    public string FileName { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public string FileType { get; set; } = "application/pdf";
    public string? FileData { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
