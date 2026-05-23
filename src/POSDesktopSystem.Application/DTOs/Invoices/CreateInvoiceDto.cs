using POSDesktopSystem.Domain.Enums;
using System.Collections.Generic;

namespace POSDesktopSystem.Application.DTOs.Invoices;

public class CreateInvoiceDto
{
    public int CashierId { get; set; }
    public decimal Discount { get; set; }
    public decimal DiscountAmount { get => Discount; set => Discount = value; }
    public decimal TaxRate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = [];
}
