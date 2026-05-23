namespace POSDesktopSystem.Application.DTOs.Products;

public class CreateProductDto
{
    public string Name { get; set; } = default!;
    public string Barcode { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Category { get; set; }
}
