using Moq;
using POSDesktopSystem.Application.DTOs.Auth;
using POSDesktopSystem.Application.DTOs.Invoices;
using POSDesktopSystem.Application.DTOs.Products;
using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Domain.Enums;
using POSDesktopSystem.Presentation.ViewModels.POS;
using POSDesktopSystem.Presentation.ViewModels.Base;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace POSDesktopSystem.Tests.ViewModels;

public class PosViewModelTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IInvoiceService> _invoiceServiceMock;
    private readonly Mock<IReceiptService> _receiptServiceMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<ISessionService> _sessionServiceMock;
    private readonly PosViewModel _viewModel;

    public PosViewModelTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _invoiceServiceMock = new Mock<IInvoiceService>();
        _receiptServiceMock = new Mock<IReceiptService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _sessionServiceMock = new Mock<ISessionService>();

        _viewModel = new PosViewModel(
            _productServiceMock.Object,
            _invoiceServiceMock.Object,
            _receiptServiceMock.Object,
            _dialogServiceMock.Object,
            _sessionServiceMock.Object);
    }

    [Fact]
    public async Task AddByBarcodeAsync_ValidBarcode_AddsToCart()
    {
        // Arrange
        var product = new ProductDto { Id = 1, Name = "Test Product", Barcode = "123", Price = 100, IsActive = true };
        _productServiceMock.Setup(x => x.GetByBarcodeAsync("123")).ReturnsAsync(product);
        _viewModel.BarcodeInput = "123";
        _viewModel.TaxRate = 0; // Set tax to 0 for simpler assertion

        // Act
        await ((IAsyncRelayCommand)_viewModel.AddByBarcodeCommand).ExecuteAsync(null);

        // Assert
        Assert.Single(_viewModel.CartItems);
        Assert.Equal(product.Id, _viewModel.CartItems[0].ProductId);
        Assert.Equal(100, _viewModel.TotalAmount);
        Assert.Empty(_viewModel.BarcodeInput);
    }

    [Fact]
    public void RecalculateTotals_CorrectCalculations()
    {
        // Arrange
        var product = new ProductDto { Id = 1, Name = "Test", Price = 100, IsActive = true };
        _viewModel.AddFromSearchCommand.Execute(product);

        // Act
        _viewModel.DiscountAmount = 10;
        _viewModel.TaxRate = 0.1m; // 10%

        // Assert
        // SubTotal = 100
        // TaxAmount = (100 - 10) * 0.1 = 9
        // TotalAmount = 100 - 10 + 9 = 99
        Assert.Equal(100, _viewModel.SubTotal);
        Assert.Equal(9, _viewModel.TaxAmount);
        Assert.Equal(99, _viewModel.TotalAmount);
    }

    [Fact]
    public async Task CheckoutAsync_ValidCart_ClearsCart()
    {
        // Arrange
        var user = new LoginResultDto { UserId = 1, Username = "cashier" };
        _sessionServiceMock.Setup(x => x.CurrentUser).Returns(user);
        
        var product = new ProductDto { Id = 1, Name = "Test", Price = 100, IsActive = true };
        _viewModel.AddFromSearchCommand.Execute(product);
        _viewModel.AmountPaid = 100;

        _invoiceServiceMock.Setup(x => x.CreateAsync(It.IsAny<CreateInvoiceDto>()))
            .ReturnsAsync(new InvoiceDto { Id = 1 });

        // Act
        await ((IAsyncRelayCommand)_viewModel.CheckoutCommand).ExecuteAsync(null);

        // Assert
        Assert.Empty(_viewModel.CartItems);
        _invoiceServiceMock.Verify(x => x.CreateAsync(It.IsAny<CreateInvoiceDto>()), Times.Once);
        _receiptServiceMock.Verify(x => x.PrintReceipt(It.IsAny<InvoiceDto>()), Times.Once);
    }
}
