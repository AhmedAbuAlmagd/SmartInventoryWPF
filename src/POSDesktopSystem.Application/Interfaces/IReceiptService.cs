using POSDesktopSystem.Application.DTOs.Invoices;

namespace POSDesktopSystem.Application.Interfaces;

public interface IReceiptService
{
    void PrintReceipt(InvoiceDto invoice);
}
