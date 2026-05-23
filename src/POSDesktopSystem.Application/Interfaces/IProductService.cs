using POSDesktopSystem.Application.DTOs.Common;
using POSDesktopSystem.Application.DTOs.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POSDesktopSystem.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto> GetByIdAsync(int id);
    Task<ProductDto?> GetByBarcodeAsync(string barcode);
    Task<PagedResultDto<ProductDto>> GetAllAsync(int page, int pageSize, string? search);
    Task<IEnumerable<ProductDto>> SearchAsync(string term);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto);
    Task DeleteAsync(int id);
}
