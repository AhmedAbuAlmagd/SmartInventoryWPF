using POSDesktopSystem.Domain.Enums;
using System;
using System.Collections.Generic;

namespace POSDesktopSystem.Domain.Entities;

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = default!;  // auto-generated: INV-YYYYMMDD-XXXX
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Open;

    public decimal SubTotal { get; set; }     // sum of all items before tax/discount
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }      // stored as decimal e.g. 0.14 for 14%
    public decimal TaxAmount { get; set; }    // calculated: (SubTotal - DiscountAmount) * TaxRate
    public decimal TotalAmount { get; set; }  // SubTotal - Discount + Tax

    public int CashierId { get; set; }
    public User Cashier { get; set; } = default!;

    public ICollection<InvoiceItem> Items { get; set; } = [];
    public Payment? Payment { get; set; }
}
