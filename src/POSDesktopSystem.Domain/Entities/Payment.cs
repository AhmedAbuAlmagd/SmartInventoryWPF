using POSDesktopSystem.Domain.Enums;
using System;

namespace POSDesktopSystem.Domain.Entities;

public class Payment : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = default!;

    public PaymentMethod Method { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Change { get; set; }       // AmountPaid - Invoice.TotalAmount (cash only)
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
}
