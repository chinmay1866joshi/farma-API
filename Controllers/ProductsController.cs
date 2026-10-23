using System.Security.Claims;
using FarmaApi.Data;
using FarmaApi.DTOs.Products;
using FarmaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// List all products with optional filters for category, search term, and organic flag
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] bool? isOrganic)
    {
        var query = _context.Products
            .Include(p => p.Farmer)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.Trim().ToLower();
            query = query.Where(p => p.Category.ToLower() == cat);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
        }

        if (isOrganic.HasValue)
        {
            query = query.Where(p => p.IsOrganic == isOrganic.Value);
        }

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category,
                PricePerUnit = p.PricePerUnit,
                Unit = p.Unit,
                QuantityAvailable = p.QuantityAvailable,
                ImageUrl = p.ImageUrl,
                IsOrganic = p.IsOrganic,
                FarmerId = p.FarmerId,
                FarmerName = p.Farmer != null ? p.Farmer.Name : "Unknown Farmer",
                FarmerCity = p.Farmer != null ? p.Farmer.City : "",
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return Ok(products);
    }

    /// <summary>
    /// Get single product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Farmer)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }

        return Ok(new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            PricePerUnit = product.PricePerUnit,
            Unit = product.Unit,
            QuantityAvailable = product.QuantityAvailable,
            ImageUrl = product.ImageUrl,
            IsOrganic = product.IsOrganic,
            FarmerId = product.FarmerId,
            FarmerName = product.Farmer?.Name ?? "Unknown Farmer",
            FarmerCity = product.Farmer?.City ?? "",
            CreatedAt = product.CreatedAt
        });
    }

    /// <summary>
    /// Add a new product (Farmers only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "farmer,admin")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var farmerId))
        {
            return Unauthorized(new { message = "Invalid farmer credentials." });
        }

        var farmer = await _context.Users.FindAsync(farmerId);
        if (farmer == null)
        {
            return Unauthorized(new { message = "Farmer account does not exist." });
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            Category = dto.Category.Trim(),
            PricePerUnit = dto.PricePerUnit,
            Unit = dto.Unit.Trim().ToLowerInvariant(),
            QuantityAvailable = dto.QuantityAvailable,
            ImageUrl = dto.ImageUrl,
            IsOrganic = dto.IsOrganic,
            FarmerId = farmerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var response = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            PricePerUnit = product.PricePerUnit,
            Unit = product.Unit,
            QuantityAvailable = product.QuantityAvailable,
            ImageUrl = product.ImageUrl,
            IsOrganic = product.IsOrganic,
            FarmerId = farmer.Id,
            FarmerName = farmer.Name,
            FarmerCity = farmer.City,
            CreatedAt = product.CreatedAt
        };

        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, response);
    }

    /// <summary>
    /// Update product (Only owner farmer or admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "farmer,admin")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        var product = await _context.Products.Include(p => p.Farmer).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (userIdClaim == null || (!Guid.TryParse(userIdClaim, out var userId) || (product.FarmerId != userId && userRole != "admin")))
        {
            return Forbid();
        }

        if (!string.IsNullOrWhiteSpace(dto.Name)) product.Name = dto.Name.Trim();
        if (dto.Description != null) product.Description = dto.Description.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Category)) product.Category = dto.Category.Trim();
        if (dto.PricePerUnit.HasValue) product.PricePerUnit = dto.PricePerUnit.Value;
        if (!string.IsNullOrWhiteSpace(dto.Unit)) product.Unit = dto.Unit.Trim().ToLowerInvariant();
        if (dto.QuantityAvailable.HasValue) product.QuantityAvailable = dto.QuantityAvailable.Value;
        if (dto.ImageUrl != null) product.ImageUrl = dto.ImageUrl;
        if (dto.IsOrganic.HasValue) product.IsOrganic = dto.IsOrganic.Value;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            PricePerUnit = product.PricePerUnit,
            Unit = product.Unit,
            QuantityAvailable = product.QuantityAvailable,
            ImageUrl = product.ImageUrl,
            IsOrganic = product.IsOrganic,
            FarmerId = product.FarmerId,
            FarmerName = product.Farmer?.Name ?? "Unknown Farmer",
            FarmerCity = product.Farmer?.City ?? "",
            CreatedAt = product.CreatedAt
        });
    }

    /// <summary>
    /// Delete product
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "farmer,admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (userIdClaim == null || (!Guid.TryParse(userIdClaim, out var userId) || (product.FarmerId != userId && userRole != "admin")))
        {
            return Forbid();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
