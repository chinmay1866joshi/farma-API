using System.ComponentModel.DataAnnotations;

namespace FarmaApi.DTOs.Orders;

public class CreateOrderItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(0.01, 100000)]
    public decimal Quantity { get; set; }
}

public class CreateOrderDto
{
    [Required]
    [MaxLength(250)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(15)]
    public string ContactPhone { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "Order must have at least one product item")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class OrderItemResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public List<OrderItemResponseDto> Items { get; set; } = new();
}
