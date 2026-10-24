using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmaApi.Models;

[Table("FarmDocuments")]
public class FarmDocument
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid FarmerId { get; set; }

    [ForeignKey(nameof(FarmerId))]
    public User? Farmer { get; set; }

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

    [Column(TypeName = "decimal(18,2)")]
    public decimal? LandAreaAcres { get; set; }

    [MaxLength(100)]
    public string? PlotNumber { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "verified";

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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
