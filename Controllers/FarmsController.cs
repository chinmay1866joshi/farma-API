using System.Security.Claims;
using FarmaApi.Data;
using FarmaApi.DTOs.Farms;
using FarmaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FarmsController : ControllerBase
{
    private readonly AppDbContext _context;

    public FarmsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FarmResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFarms()
    {
        var farms = await _context.Farms
            .Include(f => f.Farmer)
            .AsNoTracking()
            .Select(f => new FarmResponseDto
            {
                Id = f.Id,
                FarmerId = f.FarmerId,
                FarmerName = f.Farmer != null ? f.Farmer.Name : "Farmer",
                FarmName = f.FarmName,
                Location = f.Location,
                TotalAcres = f.TotalAcres,
                SoilType = f.SoilType,
                OrganicCertificationNo = f.OrganicCertificationNo,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        return Ok(farms);
    }

    [HttpPost]
    [Authorize(Roles = "farmer,admin")]
    [ProducesResponseType(typeof(FarmResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFarm([FromBody] CreateFarmDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var farmerId))
        {
            return Unauthorized();
        }

        var farmer = await _context.Users.FindAsync(farmerId);
        if (farmer == null)
        {
            return Unauthorized();
        }

        var farm = new Farm
        {
            Id = Guid.NewGuid(),
            FarmerId = farmerId,
            FarmName = dto.FarmName.Trim(),
            Location = dto.Location.Trim(),
            TotalAcres = dto.TotalAcres,
            SoilType = dto.SoilType?.Trim(),
            OrganicCertificationNo = dto.OrganicCertificationNo?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Farms.Add(farm);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAllFarms), new FarmResponseDto
        {
            Id = farm.Id,
            FarmerId = farm.FarmerId,
            FarmerName = farmer.Name,
            FarmName = farm.FarmName,
            Location = farm.Location,
            TotalAcres = farm.TotalAcres,
            SoilType = farm.SoilType,
            OrganicCertificationNo = farm.OrganicCertificationNo,
            CreatedAt = farm.CreatedAt
        });
    }
}
