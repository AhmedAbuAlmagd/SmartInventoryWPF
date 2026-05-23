using POSDesktopSystem.Application.DTOs.Invoices;
using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Domain.Enums;
using POSDesktopSystem.Presentation.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace POSDesktopSystem.Presentation.ViewModels.Invoices;

public class InvoicesViewModel : BaseViewModel
{
    private readonly IInvoiceService _invoiceService;
    private readonly IReceiptService _receiptService;
    private readonly IDialogService _dialogService;
    private readonly ISessionService _sessionService;

    public ObservableCollection<InvoiceDto> Invoices { get; } = new();

    private InvoiceDto? _selectedInvoice;
    public InvoiceDto? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            SetProperty(ref _selectedInvoice, value);
            if (ViewDetailCommand is AsyncRelayCommand viewCmd) viewCmd.RaiseCanExecuteChanged();
            if (CancelInvoiceCommand is AsyncRelayCommand cancelCmd) cancelCmd.RaiseCanExecuteChanged();
            if (ReprintCommand is AsyncRelayCommand reprintCmd) reprintCmd.RaiseCanExecuteChanged();
        }
    }

    private InvoiceStatus? _statusFilter;
    public InvoiceStatus? StatusFilter
    {
        get => _statusFilter;
        set
        {
            if (SetProperty(ref _statusFilter, value))
            {
                CurrentPage = 1;
                var _ = LoadInvoicesAsync();
            }
        }
    }

    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public int PageSize { get; } = 20;

    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        set => SetProperty(ref _totalCount, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand ViewDetailCommand { get; }
    public ICommand CancelInvoiceCommand { get; }
    public ICommand ReprintCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }

    public InvoicesViewModel(
        IInvoiceService invoiceService,
        IReceiptService receiptService,
        IDialogService dialogService,
        ISessionService sessionService)
    {
        _invoiceService = invoiceService;
        _receiptService = receiptService;
        _dialogService = dialogService;
        _sessionService = sessionService;

        LoadCommand = new AsyncRelayCommand(_ => LoadInvoicesAsync());
        ViewDetailCommand = new AsyncRelayCommand(_ => ViewDetail(), _ => SelectedInvoice != null);
        CancelInvoiceCommand = new AsyncRelayCommand(_ => CancelInvoiceAsync(), _ => SelectedInvoice?.Status == "Open" && _sessionService.IsManager);
        ReprintCommand = new AsyncRelayCommand(_ => ReprintAsync(), _ => SelectedInvoice?.Status == "Paid");

        NextPageCommand = new AsyncRelayCommand(async _ =>
        {
            CurrentPage++;
            await LoadInvoicesAsync();
        }, _ => CurrentPage * PageSize < TotalCount);

        PreviousPageCommand = new AsyncRelayCommand(async _ =>
        {
            CurrentPage--;
            await LoadInvoicesAsync();
        }, _ => CurrentPage > 1);

        // Auto-load invoices on initialization
        Task.Run(async () => await LoadInvoicesAsync());
    }

    public async Task LoadInvoicesAsync()
    {
        IsBusy = true;
        ClearError();

        try
        {
            var result = await _invoiceService.GetAllAsync(CurrentPage, PageSize, StatusFilter);
            
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Invoices.Clear();
                foreach (var item in result.Items) Invoices.Add(item);
                TotalCount = result.TotalCount;

                if (NextPageCommand is AsyncRelayCommand nextCmd) nextCmd.RaiseCanExecuteChanged();
                if (PreviousPageCommand is AsyncRelayCommand prevCmd) prevCmd.RaiseCanExecuteChanged();
            });
        }
        catch (Exception ex)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ErrorMessage = ex.Message;
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ViewDetail()
    {
        if (SelectedInvoice == null) return;
        
        IsBusy = true;
        try
        {
            var fullInvoice = await _invoiceService.GetByIdAsync(SelectedInvoice.Id);
            var vm = new InvoiceDetailViewModel(_receiptService, fullInvoice);
            _dialogService.ShowDialog<InvoiceDetailViewModel>(vm);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Could not load invoice details: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelInvoiceAsync()
    {
        if (SelectedInvoice == null) return;
        if (!_dialogService.Confirm("Cancel Invoice", $"Are you sure you want to cancel invoice {SelectedInvoice.InvoiceNumber}?")) return;

        try
        {
            await _invoiceService.CancelAsync(SelectedInvoice.Id);
            await LoadInvoicesAsync();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError("Error", ex.Message);
        }
    }

    private async Task ReprintAsync()
    {
        if (SelectedInvoice == null) return;

        try
        {
            var invoice = await _invoiceService.GetByIdAsync(SelectedInvoice.Id);
            _receiptService.PrintReceipt(invoice);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError("Print Error", ex.Message);
        }
    }
}
