using System.ComponentModel.DataAnnotations;

namespace FarmaApi.DTOs.Farms;

public class CreateFarmDto
{
    [Required]
    [MaxLength(150)]
    public string FarmName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [Range(0.1, 100000)]
    public decimal TotalAcres { get; set; }

    [MaxLength(100)]
    public string? SoilType { get; set; }

    [MaxLength(100)]
    public string? OrganicCertificationNo { get; set; }
}

public class FarmResponseDto
{
    public Guid Id { get; set; }
    public Guid FarmerId { get; set; }
    public string FarmerName { get; set; } = string.Empty;
    public string FarmName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal TotalAcres { get; set; }
    public string? SoilType { get; set; }
    public string? OrganicCertificationNo { get; set; }
    public DateTime CreatedAt { get; set; }
}
