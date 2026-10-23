using System.ComponentModel.DataAnnotations;

namespace FarmaApi.DTOs.Products;

public class CreateProductDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = "Vegetables";

    [Required]
    [Range(0.01, 1000000)]
    public decimal PricePerUnit { get; set; }

    [Required]
    [MaxLength(20)]
    public string Unit { get; set; } = "kg";

    [Required]
    [Range(0.01, 1000000)]
    public decimal QuantityAvailable { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsOrganic { get; set; } = true;
}

public class UpdateProductDto
{
    [MaxLength(150)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [Range(0.01, 1000000)]
    public decimal? PricePerUnit { get; set; }

    [MaxLength(20)]
    public string? Unit { get; set; }

    [Range(0, 1000000)]
    public decimal? QuantityAvailable { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool? IsOrganic { get; set; }
}

public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal PricePerUnit { get; set; }
    public string Unit { get; set; } = "kg";
    public decimal QuantityAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsOrganic { get; set; }
    public Guid FarmerId { get; set; }
    public string FarmerName { get; set; } = string.Empty;
    public string FarmerCity { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
