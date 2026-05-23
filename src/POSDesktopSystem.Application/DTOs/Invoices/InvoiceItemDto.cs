namespace POSDesktopSystem.Application.DTOs.Invoices;

public class InvoiceItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
