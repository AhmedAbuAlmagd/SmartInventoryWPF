using POSDesktopSystem.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POSDesktopSystem.Domain.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByBarcodeAsync(string barcode);
    Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null);
    Task<IEnumerable<Product>> SearchAsync(string term);
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search);
}
