using POSDesktopSystem.Application.DTOs.Common;
using POSDesktopSystem.Application.DTOs.Products;
using POSDesktopSystem.Application.Exceptions;
using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Domain.Entities;
using POSDesktopSystem.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace POSDesktopSystem.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException($"Product with ID {id} not found.");
        return MapToDto(product);
    }

    public async Task<ProductDto?> GetByBarcodeAsync(string barcode)
    {
        var product = await _unitOfWork.Products.GetByBarcodeAsync(barcode);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<PagedResultDto<ProductDto>> GetAllAsync(int page, int pageSize, string? search)
    {
        var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(page, pageSize, search);
        return new PagedResultDto<ProductDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<ProductDto>> SearchAsync(string term)
    {
        var products = await _unitOfWork.Products.SearchAsync(term);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        if (await _unitOfWork.Products.BarcodeExistsAsync(dto.Barcode))
            throw new ValidationException("Barcode already exists");

        if (dto.StockQuantity < 0)
            throw new ValidationException("Stock quantity cannot be negative");

        if (dto.Price <= 0 || dto.CostPrice < 0)
            throw new ValidationException("Invalid price or cost price");

        var product = new Product
        {
            Name = dto.Name,
            Barcode = dto.Barcode,
            Description = dto.Description,
            Price = dto.Price,
            CostPrice = dto.CostPrice,
            StockQuantity = dto.StockQuantity,
            Category = dto.Category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created new product: {ProductName} (Barcode: {Barcode}, Price: {Price})", 
            product.Name, product.Barcode, product.Price);

        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException($"Product with ID {id} not found.");

        if (await _unitOfWork.Products.BarcodeExistsAsync(dto.Barcode, id))
            throw new ValidationException("Barcode already exists");

        if (dto.StockQuantity < 0)
            throw new ValidationException("Stock quantity cannot be negative");

        if (dto.Price <= 0 || dto.CostPrice < 0)
            throw new ValidationException("Invalid price or cost price");

        product.Name = dto.Name;
        product.Barcode = dto.Barcode;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.CostPrice = dto.CostPrice;
        product.StockQuantity = dto.StockQuantity;
        product.Category = dto.Category;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated product ID {ProductId}: {ProductName} (Barcode: {Barcode})", 
            product.Id, product.Name, product.Barcode);

        return MapToDto(product);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException($"Product with ID {id} not found.");

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning("Soft-deleted product ID {ProductId}: {ProductName}", product.Id, product.Name);
    }

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Barcode = p.Barcode,
        Description = p.Description,
        Price = p.Price,
        CostPrice = p.CostPrice,
        StockQuantity = p.StockQuantity,
        Category = p.Category,
        IsActive = p.IsActive
    };
}
