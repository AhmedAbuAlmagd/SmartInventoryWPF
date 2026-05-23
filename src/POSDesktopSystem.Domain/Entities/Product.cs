using System.Collections.Generic;

namespace POSDesktopSystem.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Barcode { get; set; } = default!;      // unique
    public string? Description { get; set; }
    public decimal Price { get; set; }                   // selling price
    public decimal CostPrice { get; set; }               // purchase cost
    public int StockQuantity { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<InvoiceItem> InvoiceItems { get; set; } = [];
}
