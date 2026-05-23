using POSDesktopSystem.Application.DTOs.Products;
using POSDesktopSystem.Application.Exceptions;
using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Presentation.ViewModels.Base;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace POSDesktopSystem.Presentation.ViewModels.Products;

public class ProductFormViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly IDialogService _dialogService;
    private readonly int? _productId;

    public event EventHandler? CloseRequested;

    private string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    private string _barcode = string.Empty;
    public string Barcode { get => _barcode; set => SetProperty(ref _barcode, value); }

    private string? _description;
    public string? Description { get => _description; set => SetProperty(ref _description, value); }

    private decimal _price;
    public decimal Price { get => _price; set => SetProperty(ref _price, value); }

    private decimal _costPrice;
    public decimal CostPrice { get => _costPrice; set => SetProperty(ref _costPrice, value); }

    private int _stockQuantity;
    public int StockQuantity { get => _stockQuantity; set => SetProperty(ref _stockQuantity, value); }

    private string? _category;
    public string? Category { get => _category; set => SetProperty(ref _category, value); }

    private bool _isActive = true;
    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public ProductFormViewModel(IProductService productService, IDialogService dialogService, ProductDto? product = null)
    {
        _productService = productService;
        _dialogService = dialogService;

        if (product != null)
        {
            _productId = product.Id;
            Name = product.Name;
            Barcode = product.Barcode;
            Description = product.Description;
            Price = product.Price;
            CostPrice = product.CostPrice;
            StockQuantity = product.StockQuantity;
            Category = product.Category;
            IsActive = product.IsActive;
        }

        SaveCommand = new AsyncRelayCommand(SaveAsync, _ => CanSave());
        CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Name) && 
               !string.IsNullOrWhiteSpace(Barcode) && 
               Price >= 0 && 
               CostPrice >= 0 && 
               StockQuantity >= 0;
    }

    private async Task SaveAsync(object? parameter)
    {
        ClearError();
        IsBusy = true;

        try
        {
            if (_productId.HasValue)
            {
                var dto = new UpdateProductDto
                {
                    Name = Name,
                    Barcode = Barcode,
                    Description = Description,
                    Price = Price,
                    CostPrice = CostPrice,
                    StockQuantity = StockQuantity,
                    Category = Category,
                    IsActive = IsActive
                };
                await _productService.UpdateAsync(_productId.Value, dto);
            }
            else
            {
                var dto = new CreateProductDto
                {
                    Name = Name,
                    Barcode = Barcode,
                    Description = Description,
                    Price = Price,
                    CostPrice = CostPrice,
                    StockQuantity = StockQuantity,
                    Category = Category
                };
                await _productService.CreateAsync(dto);
            }

            CloseRequested?.Invoke(this, EventArgs.Empty);
            _dialogService.ShowInfo("Success", "Product saved successfully.");
        }
        catch (ValidationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
