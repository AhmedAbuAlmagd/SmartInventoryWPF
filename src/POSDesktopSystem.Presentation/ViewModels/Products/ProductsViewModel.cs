using POSDesktopSystem.Application.DTOs.Products;
using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Presentation.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System;

namespace POSDesktopSystem.Presentation.ViewModels.Products;

public class ProductsViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;
    private DispatcherTimer _debounceTimer;

    public ObservableCollection<ProductDto> Products { get; } = new();

    private ProductDto? _selectedProduct;
    public ProductDto? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            SetProperty(ref _selectedProduct, value);
            ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
            ((AsyncRelayCommand)DeleteCommand).RaiseCanExecuteChanged();
        }
    }

    private string _searchTerm = string.Empty;
    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (SetProperty(ref _searchTerm, value))
            {
                _debounceTimer.Stop();
                _debounceTimer.Start();
            }
        }
    }

    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        set => SetProperty(ref _totalCount, value);
    }

    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public int PageSize { get; } = 15;

    public ICommand LoadCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }

    public ProductsViewModel(IProductService productService, IDialogService dialogService, IServiceProvider serviceProvider)
    {
        _productService = productService;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;

        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _debounceTimer.Tick += async (s, e) =>
        {
            _debounceTimer.Stop();
            CurrentPage = 1;
            await LoadProductsAsync();
        };

        LoadCommand = new AsyncRelayCommand(_ => LoadProductsAsync());
        AddCommand = new RelayCommand(_ => AddProduct());
        EditCommand = new RelayCommand(_ => EditProduct(), _ => SelectedProduct != null);
        DeleteCommand = new AsyncRelayCommand(_ => DeleteProductAsync(), _ => SelectedProduct != null);

        NextPageCommand = new AsyncRelayCommand(async _ =>
        {
            CurrentPage++;
            await LoadProductsAsync();
        }, _ => CurrentPage * PageSize < TotalCount);

        PreviousPageCommand = new AsyncRelayCommand(async _ =>
        {
            CurrentPage--;
            await LoadProductsAsync();
        }, _ => CurrentPage > 1);

        // Auto-load on init
        Task.Run(async () => await LoadProductsAsync());
    }

    public async Task LoadProductsAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _productService.GetAllAsync(CurrentPage, PageSize, SearchTerm);
            
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Products.Clear();
                foreach (var item in result.Items) Products.Add(item);
                TotalCount = result.TotalCount;

                if (NextPageCommand is AsyncRelayCommand nextCmd) nextCmd.RaiseCanExecuteChanged();
                if (PreviousPageCommand is AsyncRelayCommand prevCmd) prevCmd.RaiseCanExecuteChanged();
            });
        }
        catch (Exception ex)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ErrorMessage = "Error loading products: " + ex.Message;
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AddProduct()
    {
        var vm = new ProductFormViewModel(_productService, _dialogService);
        vm.CloseRequested += async (s, e) => await LoadProductsAsync();
        _dialogService.ShowDialog<ProductFormViewModel>(vm);
    }

    private void EditProduct()
    {
        if (SelectedProduct == null) return;
        var vm = new ProductFormViewModel(_productService, _dialogService, SelectedProduct);
        vm.CloseRequested += async (s, e) => await LoadProductsAsync();
        _dialogService.ShowDialog<ProductFormViewModel>(vm);
    }

    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;
        if (!_dialogService.Confirm("Delete Product", $"Are you sure you want to delete {SelectedProduct.Name}?")) return;

        try
        {
            await _productService.DeleteAsync(SelectedProduct.Id);
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError("Error", ex.Message);
        }
    }
}
