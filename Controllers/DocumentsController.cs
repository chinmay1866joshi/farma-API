using System.Security.Claims;
using FarmaApi.Data;
using FarmaApi.DTOs.Documents;
using FarmaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public DocumentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocumentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDocuments([FromQuery] string? category = null)
    {
        var query = _context.FarmDocuments
            .Include(d => d.Farmer)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category) && category.ToLower() != "all")
        {
            query = query.Where(d => d.Category.ToLower() == category.ToLower().Trim());
        }

        var docs = await query
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DocumentResponseDto
            {
                Id = d.Id,
                FarmerId = d.FarmerId,
                FarmerName = d.Farmer != null ? d.Farmer.Name : "Kafh Farmer",
                Title = d.Title,
                Category = d.Category,
                DocumentNumber = d.DocumentNumber,
                IssuingAuthority = d.IssuingAuthority,
                IssueDate = d.IssueDate,
                ExpiryDate = d.ExpiryDate,
                LandAreaAcres = d.LandAreaAcres,
                PlotNumber = d.PlotNumber,
                Status = d.Status,
                FileName = d.FileName,
                FileSize = d.FileSize,
                FileType = d.FileType,
                FileData = null, // Excluded from list query for instant network response; loaded on-demand in GetDocumentById
                Notes = d.Notes,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();

        return Ok(docs);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentById(Guid id)
    {
        var doc = await _context.FarmDocuments
            .Include(d => d.Farmer)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doc == null)
        {
            return NotFound(new { message = "Document not found." });
        }

        return Ok(new DocumentResponseDto
        {
            Id = doc.Id,
            FarmerId = doc.FarmerId,
            FarmerName = doc.Farmer != null ? doc.Farmer.Name : "Kafh Farmer",
            Title = doc.Title,
            Category = doc.Category,
            DocumentNumber = doc.DocumentNumber,
            IssuingAuthority = doc.IssuingAuthority,
            IssueDate = doc.IssueDate,
            ExpiryDate = doc.ExpiryDate,
            LandAreaAcres = doc.LandAreaAcres,
            PlotNumber = doc.PlotNumber,
            Status = doc.Status,
            FileName = doc.FileName,
            FileSize = doc.FileSize,
            FileType = doc.FileType,
            FileData = doc.FileData,
            Notes = doc.Notes,
            CreatedAt = doc.CreatedAt
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Try getting farmer ID from JWT claims
        Guid farmerId = Guid.Empty;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var claimGuid))
        {
            farmerId = claimGuid;
        }

        // Fallback: If not found or anonymous test, link to first farmer in database
        if (farmerId == Guid.Empty)
        {
            var defaultFarmer = await _context.Users.FirstOrDefaultAsync(u => u.Role == "farmer")
                             ?? await _context.Users.FirstOrDefaultAsync();

            if (defaultFarmer != null)
            {
                farmerId = defaultFarmer.Id;
            }
            else
            {
                // Create a default system farmer if none exists
                var sysFarmer = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Sanjivani Farmer",
                    Email = "farmer@kafhorganic.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    MobileNumber = "9876543210",
                    Age = 35,
                    Role = "farmer",
                    City = "Pune",
                    State = "Maharashtra",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(sysFarmer);
                await _context.SaveChangesAsync();
                farmerId = sysFarmer.Id;
            }
        }

        var farmer = await _context.Users.FindAsync(farmerId);

        var doc = new FarmDocument
        {
            Id = Guid.NewGuid(),
            FarmerId = farmerId,
            Title = dto.Title.Trim(),
            Category = dto.Category.Trim().ToLower(),
            DocumentNumber = dto.DocumentNumber.Trim().ToUpper(),
            IssuingAuthority = dto.IssuingAuthority.Trim(),
            IssueDate = dto.IssueDate.Trim(),
            ExpiryDate = !string.IsNullOrWhiteSpace(dto.ExpiryDate) ? dto.ExpiryDate.Trim() : null,
            LandAreaAcres = dto.LandAreaAcres,
            PlotNumber = !string.IsNullOrWhiteSpace(dto.PlotNumber) ? dto.PlotNumber.Trim() : null,
            Status = "verified",
            FileName = dto.FileName.Trim(),
            FileSize = dto.FileSize.Trim(),
            FileType = !string.IsNullOrWhiteSpace(dto.FileType) ? dto.FileType.Trim() : "application/pdf",
            FileData = dto.FileData,
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.FarmDocuments.Add(doc);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDocumentById), new { id = doc.Id }, new DocumentResponseDto
        {
            Id = doc.Id,
            FarmerId = doc.FarmerId,
            FarmerName = farmer != null ? farmer.Name : "Kafh Farmer",
            Title = doc.Title,
            Category = doc.Category,
            DocumentNumber = doc.DocumentNumber,
            IssuingAuthority = doc.IssuingAuthority,
            IssueDate = doc.IssueDate,
            ExpiryDate = doc.ExpiryDate,
            LandAreaAcres = doc.LandAreaAcres,
            PlotNumber = doc.PlotNumber,
            Status = doc.Status,
            FileName = doc.FileName,
            FileSize = doc.FileSize,
            FileType = doc.FileType,
            FileData = doc.FileData,
            Notes = doc.Notes,
            CreatedAt = doc.CreatedAt
        });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        var doc = await _context.FarmDocuments.FindAsync(id);
        if (doc == null)
        {
            return NotFound(new { message = "Document not found." });
        }

        _context.FarmDocuments.Remove(doc);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Document deleted successfully." });
    }
}
