namespace POSDesktopSystem.Domain.Entities;

public class InvoiceItem : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = default!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public string ProductName { get; set; } = default!;  // snapshot at time of sale
    public decimal UnitPrice { get; set; }               // snapshot at time of sale
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }               // UnitPrice * Quantity
}
