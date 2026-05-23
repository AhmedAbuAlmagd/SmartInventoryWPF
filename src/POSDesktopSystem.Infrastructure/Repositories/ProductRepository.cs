using Microsoft.EntityFrameworkCore;
using POSDesktopSystem.Domain.Entities;
using POSDesktopSystem.Domain.Interfaces.Repositories;
using POSDesktopSystem.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POSDesktopSystem.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null)
    {
        return await _dbSet.IgnoreQueryFilters()
            .AnyAsync(p => p.Barcode == barcode && (!excludeId.HasValue || p.Id != excludeId.Value));
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Barcode == barcode);
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.Barcode.Contains(search));
        }

        int totalCount = await query.CountAsync();
        var items = await query.OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return new List<Product>();

        return await _dbSet
            .Where(p => p.Name.Contains(term) || p.Barcode.Contains(term))
            .Take(20)
            .ToListAsync();
    }
}
