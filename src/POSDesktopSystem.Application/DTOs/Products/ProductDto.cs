namespace POSDesktopSystem.Application.DTOs.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Barcode { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
}
