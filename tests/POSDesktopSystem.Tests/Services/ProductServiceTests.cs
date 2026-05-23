using Moq;
using Microsoft.Extensions.Logging;
using POSDesktopSystem.Application.DTOs.Products;
using POSDesktopSystem.Application.Exceptions;
using POSDesktopSystem.Application.Services;
using POSDesktopSystem.Domain.Entities;
using POSDesktopSystem.Domain.Interfaces;
using Xunit;

namespace POSDesktopSystem.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProductService>>();
        _service = new ProductService(_uowMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsProductDto()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Barcode = "123" };
        _uowMock.Setup(x => x.Products.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonexistentProduct_ThrowsNotFoundException()
    {
        // Arrange
        _uowMock.Setup(x => x.Products.GetByIdAsync(1)).ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(1));
    }

    [Fact]
    public async Task CreateAsync_DuplicateBarcode_ThrowsValidationException()
    {
        // Arrange
        _uowMock.Setup(x => x.Products.BarcodeExistsAsync("123", null)).ReturnsAsync(true);
        var dto = new CreateProductDto { Barcode = "123" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(dto));
        Assert.Equal("Barcode already exists", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_NegativeStock_ThrowsValidationException()
    {
        // Arrange
        _uowMock.Setup(x => x.Products.BarcodeExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
        var dto = new CreateProductDto { Barcode = "123", StockQuantity = -1 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(dto));
        Assert.Equal("Stock quantity cannot be negative", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_ExistingProduct_SetsIsActiveToFalse()
    {
        // Arrange
        var product = new Product { Id = 1, IsActive = true };
        _uowMock.Setup(x => x.Products.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        Assert.False(product.IsActive);
        _uowMock.Verify(x => x.Products.Update(product), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
