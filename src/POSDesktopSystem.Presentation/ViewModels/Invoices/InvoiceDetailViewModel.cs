using POSDesktopSystem.Application.DTOs.Invoices;
using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Presentation.ViewModels.Base;
using System;
using System.Windows.Input;

namespace POSDesktopSystem.Presentation.ViewModels.Invoices;

public class InvoiceDetailViewModel : BaseViewModel
{
    private readonly IReceiptService _receiptService;

    public InvoiceDto Invoice { get; }

    public ICommand PrintCommand { get; }
    public ICommand CloseCommand { get; }

    public event EventHandler? CloseRequested;

    public InvoiceDetailViewModel(IReceiptService receiptService, InvoiceDto invoice)
    {
        _receiptService = receiptService;
        Invoice = invoice;

        PrintCommand = new RelayCommand(_ => PrintReceipt());
        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
    }

    private void PrintReceipt()
    {
        try
        {
            _receiptService.PrintReceipt(Invoice);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Printing failed: " + ex.Message;
        }
    }
}
