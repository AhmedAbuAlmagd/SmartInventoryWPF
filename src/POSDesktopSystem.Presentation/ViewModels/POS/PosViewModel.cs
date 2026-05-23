using POSDesktopSystem.Application.DTOs.Invoices;
using POSDesktopSystem.Application.DTOs.Products;
using POSDesktopSystem.Application.Exceptions;
using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Domain.Enums;
using POSDesktopSystem.Presentation.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace POSDesktopSystem.Presentation.ViewModels.POS;

public class PosViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly IInvoiceService _invoiceService;
    private readonly IReceiptService _receiptService;
    private readonly IDialogService _dialogService;
    private readonly ISessionService _sessionService;

    public ObservableCollection<ProductDto> SearchResults { get; } = new();
    public ObservableCollection<CartItemViewModel> CartItems { get; } = new();

    private ProductDto? _selectedSearchResult;
    public ProductDto? SelectedSearchResult
    {
        get => _selectedSearchResult;
        set => SetProperty(ref _selectedSearchResult, value);
    }

    private string _barcodeInput = string.Empty;
    public string BarcodeInput
    {
        get => _barcodeInput;
        set => SetProperty(ref _barcodeInput, value);
    }

    private string _searchTerm = string.Empty;
    private System.Threading.CancellationTokenSource? _searchCts;

    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (SetProperty(ref _searchTerm, value))
            {
                _searchCts?.Cancel();
                _searchCts = new System.Threading.CancellationTokenSource();
                var token = _searchCts.Token;

                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(300, token); // Debounce for 300ms
                        if (!token.IsCancellationRequested)
                        {
                            await SearchProductsAsync(null);
                        }
                    }
                    catch (OperationCanceledException) { }
                }, token);
            }
        }
    }

    private decimal _discountAmount;
    public decimal DiscountAmount
    {
        get => _discountAmount;
        set
        {
            if (SetProperty(ref _discountAmount, value)) RecalculateTotals();
        }
    }

    private decimal _taxRate = 0.14m;
    public decimal TaxRate
    {
        get => _taxRate;
        set
        {
            if (SetProperty(ref _taxRate, value)) RecalculateTotals();
        }
    }

    public decimal SubTotal => CartItems.Sum(x => x.LineTotal);
    public decimal TaxAmount => (SubTotal - DiscountAmount) * TaxRate;
    public decimal TotalAmount => SubTotal - DiscountAmount + TaxAmount;

    private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;
    public PaymentMethod SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set
        {
            if (SetProperty(ref _selectedPaymentMethod, value))
            {
                if (value == PaymentMethod.Card)
                    AmountPaid = TotalAmount;
                OnPropertyChanged(nameof(Change));
                EvaluateCheckOut();
            }
        }
    }

    private decimal? _amountPaid;
    public decimal? AmountPaid
    {
        get => _amountPaid;
        set
        {
            if (SetProperty(ref _amountPaid, value))
            {
                OnPropertyChanged(nameof(Change));
                EvaluateCheckOut();
            }
        }
    }

    public decimal Change => (SelectedPaymentMethod == PaymentMethod.Cash && AmountPaid.HasValue) ? AmountPaid.Value - TotalAmount : 0;

    public bool IsDiscountEditable => true; // Enabled for both Manager and Cashier
    public bool IsTaxEditable => _sessionService.IsManager;
    
    public IEnumerable<PaymentMethod> AvailablePaymentMethods => Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>();

    private bool _canCheckout;
    public bool CanCheckout
    {
        get => _canCheckout;
        private set => SetProperty(ref _canCheckout, value);
    }

    public ICommand AddByBarcodeCommand { get; }
    public ICommand SearchProductsCommand { get; }
    public ICommand AddFromSearchCommand { get; }
    public ICommand RemoveCartItemCommand { get; }
    public ICommand IncrementQuantityCommand { get; }
    public ICommand DecrementQuantityCommand { get; }
    public ICommand CheckoutCommand { get; }
    public ICommand ClearCartCommand { get; }

    public PosViewModel(
        IProductService productService,
        IInvoiceService invoiceService,
        IReceiptService receiptService,
        IDialogService dialogService,
        ISessionService sessionService)
    {
        _productService = productService;
        _invoiceService = invoiceService;
        _receiptService = receiptService;
        _dialogService = dialogService;
        _sessionService = sessionService;

        CartItems.CollectionChanged += (s, e) => RecalculateTotals();

        AddByBarcodeCommand = new AsyncRelayCommand(AddByBarcodeAsync);
        SearchProductsCommand = new AsyncRelayCommand(SearchProductsAsync);
        AddFromSearchCommand = new RelayCommand(AddFromSearch);
        RemoveCartItemCommand = new RelayCommand(RemoveCartItem);
        IncrementQuantityCommand = new RelayCommand(IncrementQuantity);
        DecrementQuantityCommand = new RelayCommand(DecrementQuantity);
        CheckoutCommand = new AsyncRelayCommand(CheckoutAsync, _ => CanCheckout);
        ClearCartCommand = new RelayCommand(_ => ClearCart());

        // Initialize totals
        RecalculateTotals();
    }

    private async Task AddByBarcodeAsync(object? parameter)
    {
        if (string.IsNullOrWhiteSpace(BarcodeInput)) return;
        ClearError();

        try
        {
            var product = await _productService.GetByBarcodeAsync(BarcodeInput);
            if (product == null)
            {
                ErrorMessage = $"Product with barcode '{BarcodeInput}' not found";
            }
            else
            {
                AddToCart(product);
                BarcodeInput = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private async Task SearchProductsAsync(object? parameter)
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => SearchResults.Clear());
            return;
        }

        ClearError();

        try
        {
            var results = await _productService.SearchAsync(SearchTerm);
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                SearchResults.Clear();
                foreach (var r in results) SearchResults.Add(r);
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void AddFromSearch(object? parameter)
    {
        if (parameter is ProductDto product)
        {
            AddToCart(product);
            SearchResults.Remove(product);
            // Automatically set Amount Paid if it was empty or 0 to help the user
            if (!AmountPaid.HasValue || AmountPaid.Value == 0)
            {                
                AmountPaid = TotalAmount;
            }
        }
    }

    private void AddToCart(ProductDto product)
    {
        if (!product.IsActive)
        {
            ErrorMessage = $"Product {product.Name} is not active.";
            return;
        }

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var existing = CartItems.FirstOrDefault(c => c.ProductId == product.Id);
            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                var item = new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Barcode = product.Barcode,
                    UnitPrice = product.Price,
                    Quantity = 1
                };
                item.PropertyChanged += (s, e) => { 
                    if (e.PropertyName == nameof(CartItemViewModel.LineTotal)) 
                    {
                        RecalculateTotals();
                        // Keep Amount Paid synced if it matches the previous total
                        if (SelectedPaymentMethod == PaymentMethod.Card) AmountPaid = TotalAmount;
                    }
                };
                CartItems.Add(item);
            }
        });
    }

    private void RemoveCartItem(object? parameter)
    {
        if (parameter is CartItemViewModel item)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                CartItems.Remove(item);
            });
        }
    }

    private void IncrementQuantity(object? parameter)
    {
        if (parameter is CartItemViewModel item)
        {
            item.Quantity++;
        }
    }

    private void DecrementQuantity(object? parameter)
    {
        if (parameter is CartItemViewModel item && item.Quantity > 1)
        {
            item.Quantity--;
        }
    }

    private decimal _lastTotal;

    private void RecalculateTotals()
    {
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(TaxAmount));
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(Change));

        // Sync Amount Paid if it was previously matching the total (automatic update)
        // or if it's currently 0/null
        if (SelectedPaymentMethod == PaymentMethod.Card || 
            !AmountPaid.HasValue || 
            AmountPaid.Value == 0 || 
            AmountPaid.Value == _lastTotal)
        {
            AmountPaid = TotalAmount;
        }
        else if (CartItems.Count == 0)
        {
            AmountPaid = 0;
        }

        _lastTotal = TotalAmount;
        EvaluateCheckOut();
    }

    private void EvaluateCheckOut()
    {
        CanCheckout = CartItems.Count > 0 && 
                      (SelectedPaymentMethod == PaymentMethod.Card || (AmountPaid.HasValue && AmountPaid.Value >= TotalAmount));
        
        if (CheckoutCommand is AsyncRelayCommand asyncCommand)
        {
            asyncCommand.RaiseCanExecuteChanged();
        }
    }

    private void ClearCart()
    {
        if (CartItems.Count == 0) return;
        if (_dialogService.Confirm("Clear Cart", "Are you sure you want to clear the cart?"))
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                CartItems.Clear();
                BarcodeInput = string.Empty;
                SearchTerm = string.Empty;
                SearchResults.Clear();
                DiscountAmount = 0;
                AmountPaid = 0;
                ClearError();
            });
            RecalculateTotals();
        }
    }

    private async Task CheckoutAsync(object? parameter)
    {
        if (CartItems.Count == 0)
        {
            ErrorMessage = "Cart is empty.";
            return;
        }

        if (!AmountPaid.HasValue || AmountPaid.Value < TotalAmount)
        {
            ErrorMessage = "Insufficient amount paid.";
            return;
        }

        IsBusy = true;
        ClearError();

        try
        {
            var invoiceDto = new CreateInvoiceDto
            {
                CashierId = _sessionService.CurrentUser?.UserId ?? 0,
                PaymentMethod = SelectedPaymentMethod,
                AmountPaid = AmountPaid.Value,
                Discount = DiscountAmount,
                TaxRate = 0.14m, // Fixed for now
                Items = CartItems.Select(c => new InvoiceItemDto
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity
                }).ToList()
            };

            var result = await _invoiceService.CreateAsync(invoiceDto);
            
            // Background print
            _receiptService.PrintReceipt(result);

            // UI feedback
            System.Windows.MessageBox.Show($"Sale completed successfully!\nInvoice: {result.InvoiceNumber}\nChange: {result.Payment?.Change ?? 0:N2}", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                CartItems.Clear();
                AmountPaid = null;
                DiscountAmount = 0;
                BarcodeInput = string.Empty;
                SearchTerm = string.Empty;
                SearchResults.Clear();
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = "Checkout failed: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
