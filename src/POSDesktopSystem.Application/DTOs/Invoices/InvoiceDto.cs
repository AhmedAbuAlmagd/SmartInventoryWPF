using System;
using System.Collections.Generic;

namespace POSDesktopSystem.Application.DTOs.Invoices;

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public DateTime InvoiceDate { get; set; }
    public string Status { get; set; } = default!;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string CashierName { get; set; } = default!;
    public List<InvoiceItemDto> Items { get; set; } = [];
    public string? PaymentMethod { get; set; }
    public decimal? AmountPaid { get; set; }
    public decimal? Change { get; set; }
    public InvoicePaymentDto? Payment { get; set; }
}

public class InvoicePaymentDto
{
    public string Method { get; set; } = default!;
    public decimal AmountPaid { get; set; }
    public decimal Change { get; set; }
}
