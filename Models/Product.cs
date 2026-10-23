using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmaApi.Models;

[Table("Products")]
public class Product
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = "Vegetables"; // Vegetables, Fruits, Grains, Dairy, Organic Seeds

    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerUnit { get; set; }

    [Required]
    [MaxLength(20)]
    public string Unit { get; set; } = "kg"; // kg, quintal, bunch, crate, litre

    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityAvailable { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsOrganic { get; set; } = true;

    [Required]
    public Guid FarmerId { get; set; }

    [ForeignKey(nameof(FarmerId))]
    public User? Farmer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
