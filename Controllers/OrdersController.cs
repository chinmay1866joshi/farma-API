using System.Security.Claims;
using FarmaApi.Data;
using FarmaApi.DTOs.Orders;
using FarmaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(AppDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Place a new order for farm products
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid || dto.Items.Count == 0)
        {
            return BadRequest(new { message = "Valid order details and at least one item are required." });
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var customerId))
        {
            return Unauthorized(new { message = "Invalid customer identification." });
        }

        var customer = await _context.Users.FindAsync(customerId);
        if (customer == null)
        {
            return Unauthorized(new { message = "Customer account not found." });
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            DeliveryAddress = dto.DeliveryAddress.Trim(),
            ContactPhone = dto.ContactPhone.Trim(),
            Status = "Pending"
        };

        decimal total = 0;

        foreach (var item in dto.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                return BadRequest(new { message = $"Product with ID {item.ProductId} does not exist." });
            }

            if (product.QuantityAvailable < item.Quantity)
            {
                return BadRequest(new { message = $"Insufficient inventory for {product.Name}. Available: {product.QuantityAvailable} {product.Unit}." });
            }

            var lineTotal = item.Quantity * product.PricePerUnit;
            total += lineTotal;

            // Reduce inventory
            product.QuantityAvailable -= item.Quantity;

            order.OrderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.PricePerUnit,
                TotalPrice = lineTotal
            });
        }

        order.TotalAmount = total;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var response = await GetOrderResponseById(order.Id);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, response);
    }

    /// <summary>
    /// Get orders for the logged-in user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsNoTracking();

        if (userRole == "farmer")
        {
            // Farmers see orders that contain any of their products
            query = query.Where(o => o.OrderItems.Any(oi => oi.Product != null && oi.Product.FarmerId == userId));
        }
        else if (userRole != "admin")
        {
            // Regular customers see their own orders
            query = query.Where(o => o.CustomerId == userId);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderResponseDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.Name : "Unknown",
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                DeliveryAddress = o.DeliveryAddress,
                ContactPhone = o.ContactPhone,
                Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product != null ? oi.Product.Name : "Product",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            })
            .ToListAsync();

        return Ok(orders);
    }

    /// <summary>
    /// Get single order by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var response = await GetOrderResponseById(id);
        if (response == null)
        {
            return NotFound(new { message = $"Order with ID {id} not found." });
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            if (userRole != "admin" && response.CustomerId != userId && userRole != "farmer")
            {
                return Forbid();
            }
        }

        return Ok(response);
    }

    private async Task<OrderResponseDto?> GetOrderResponseById(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new OrderResponseDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.Name : "Unknown",
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                DeliveryAddress = o.DeliveryAddress,
                ContactPhone = o.ContactPhone,
                Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product != null ? oi.Product.Name : "Product",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }
}
