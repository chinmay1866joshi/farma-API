using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmaApi.Models;

[Table("Farms")]
public class Farm
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid FarmerId { get; set; }

    [ForeignKey(nameof(FarmerId))]
    public User? Farmer { get; set; }

    [Required]
    [MaxLength(150)]
    public string FarmName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAcres { get; set; }

    [MaxLength(100)]
    public string? SoilType { get; set; }

    [MaxLength(100)]
    public string? OrganicCertificationNo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
