using Moq;
using Microsoft.Extensions.Logging;
using POSDesktopSystem.Application.DTOs.Invoices;
using POSDesktopSystem.Application.Exceptions;
using POSDesktopSystem.Application.Services;
using POSDesktopSystem.Domain.Entities;
using POSDesktopSystem.Domain.Enums;
using POSDesktopSystem.Domain.Interfaces;
using POSDesktopSystem.Domain.Interfaces.Repositories;
using Xunit;

namespace POSDesktopSystem.Tests.Services;

public class InvoiceServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
    private readonly Mock<ILogger<InvoiceService>> _loggerMock;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _productRepoMock = new Mock<IProductRepository>();
        _invoiceRepoMock = new Mock<IInvoiceRepository>();
        _loggerMock = new Mock<ILogger<InvoiceService>>();

        _uowMock.Setup(x => x.Products).Returns(_productRepoMock.Object);
        _uowMock.Setup(x => x.Invoices).Returns(_invoiceRepoMock.Object);

        _service = new InvoiceService(_uowMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_EmptyItems_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateInvoiceDto { Items = new List<InvoiceItemDto>() };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_InsufficientStock_ThrowsInsufficientStockException()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test", Price = 10, StockQuantity = 5, IsActive = true };
        _productRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
        _invoiceRepoMock.Setup(x => x.GenerateInvoiceNumberAsync()).ReturnsAsync("INV-001");

        var dto = new CreateInvoiceDto
        {
            Items = new List<InvoiceItemDto> { new InvoiceItemDto { ProductId = 1, Quantity = 10 } }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InsufficientStockException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ValidInvoice_ReturnsInvoiceDto()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test", Price = 100, StockQuantity = 10, IsActive = true };
        _productRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
        _invoiceRepoMock.Setup(x => x.GenerateInvoiceNumberAsync()).ReturnsAsync("INV-001");

        var dto = new CreateInvoiceDto
        {
            CashierId = 1,
            PaymentMethod = PaymentMethod.Cash,
            AmountPaid = 100,
            TaxRate = 0,
            Items = new List<InvoiceItemDto> { new InvoiceItemDto { ProductId = 1, Quantity = 1 } }
        };

        // Set up the mock to return the created invoice with details
        _invoiceRepoMock.Setup(x => x.GetWithDetailsAsync(It.IsAny<int>()))
            .ReturnsAsync(new Invoice 
            { 
                Id = 1, 
                InvoiceNumber = "INV-001",
                SubTotal = 100,
                TotalAmount = 100,
                Items = new List<InvoiceItem> { new InvoiceItem { ProductName = "Test", Quantity = 1, UnitPrice = 100, LineTotal = 100 } },
                Payment = new Payment { Method = PaymentMethod.Cash, AmountPaid = 100, Change = 0 }
            });

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("INV-001", result.InvoiceNumber);
        Assert.Equal(100, result.TotalAmount);
        _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
