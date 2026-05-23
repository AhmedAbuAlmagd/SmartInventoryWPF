using POSDesktopSystem.Application.DTOs.Invoices;
using POSDesktopSystem.Application.Exceptions;
using POSDesktopSystem.Application.Interfaces;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;

namespace POSDesktopSystem.Infrastructure.Printing;

public class ReceiptPrinter : IReceiptService
{
    public void PrintReceipt(InvoiceDto invoice)
    {
        try
        {
            var pd = new PrintDocument();
            pd.PrintPage += (sender, e) => PrintPageHandler(e, invoice);
            pd.Print();
        }
        catch (Exception ex)
        {
            throw new AppException("Printing failed: " + ex.Message, ex);
        }
    }

    private void PrintPageHandler(PrintPageEventArgs e, InvoiceDto invoice)
    {
        var g = e.Graphics;
        using var font = new Font("Consolas", 10);
        using var boldFont = new Font("Consolas", 10, FontStyle.Bold);
        using var brush = new SolidBrush(Color.Black);
        float y = 10;
        float x = 10;
        int width = 280; // Assuming 80mm thermal paper approx

        void PrintLine(string text, Font f, float lineY, StringFormat? af = null) => g?.DrawString(text, f, brush, new RectangleF(x, lineY, width, 20), af ?? new StringFormat());

        var centerFormat = new StringFormat { Alignment = StringAlignment.Center };
        var rightFormat = new StringFormat { Alignment = StringAlignment.Far };

        PrintLine("================================", font, y += 20, centerFormat);
        PrintLine("SMART POS SYSTEM", boldFont, y += 20, centerFormat);
        PrintLine("================================", font, y += 20, centerFormat);
        
        PrintLine($"Invoice: {invoice.InvoiceNumber}", font, y += 20);
        PrintLine($"Date: {invoice.InvoiceDate:dd/MM/yyyy hh:mm tt}", font, y += 20);
        PrintLine($"Cashier: {invoice.CashierName}", font, y += 20);
        PrintLine("--------------------------------", font, y += 20);
        
        PrintLine("Item Name         Qty    Price", boldFont, y += 20);
        PrintLine("--------------------------------", font, y += 20);
        
        foreach (var item in invoice.Items)
        {
            PrintLine($"{item.ProductName?.PadRight(15).Substring(0, Math.Min(15, item.ProductName?.Length ?? 0))} {item.Quantity.ToString().PadLeft(3)} {item.UnitPrice,8:0.00}", font, y += 20);
        }
        PrintLine("--------------------------------", font, y += 20);
        
        PrintLine("Subtotal:", font, y += 20); PrintLine($"EGP {invoice.SubTotal:0.00}", font, y, rightFormat);
        PrintLine("Discount:", font, y += 20); PrintLine($"-EGP {invoice.DiscountAmount:0.00}", font, y, rightFormat);
        PrintLine($"Tax ({invoice.TaxRate*100}%):", font, y += 20); PrintLine($"EGP {invoice.TaxAmount:0.00}", font, y, rightFormat);
        PrintLine("TOTAL:", boldFont, y += 20); PrintLine($"EGP {invoice.TotalAmount:0.00}", boldFont, y, rightFormat);
        
        PrintLine("================================", font, y += 20);
        PrintLine($"Payment: {invoice.PaymentMethod}", font, y += 20);
        if (invoice.AmountPaid.HasValue)
        {
            PrintLine("Paid:", font, y += 20); PrintLine($"EGP {invoice.AmountPaid.Value:0.00}", font, y, rightFormat);
        }
        if (invoice.Change.HasValue && invoice.Change.Value > 0)
        {
            PrintLine("Change:", font, y += 20); PrintLine($"EGP {invoice.Change.Value:0.00}", font, y, rightFormat);
        }
        PrintLine("================================", font, y += 20);
        PrintLine("Thank you for your visit!", boldFont, y += 20, centerFormat);
        PrintLine("================================", font, y += 20, centerFormat);
    }
}
