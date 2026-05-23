using Microsoft.EntityFrameworkCore;
using POSDesktopSystem.Domain.Entities;
using POSDesktopSystem.Domain.Enums;
using POSDesktopSystem.Domain.Interfaces.Repositories;
using POSDesktopSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POSDesktopSystem.Infrastructure.Repositories;

public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(AppDbContext context) : base(context) { }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        
        var count = await _dbSet.Where(i => i.InvoiceDate >= today && i.InvoiceDate < tomorrow).CountAsync();
        return $"INV-{today:yyyyMMdd}-{(count + 1):D4}";
    }

    public async Task<(IEnumerable<Invoice> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, InvoiceStatus? status)
    {
        var query = _dbSet
            .Include(i => i.Cashier)
            .Include(i => i.Payment)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        int totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Invoice>> GetTodaysInvoicesAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        
        return await _dbSet
            .Include(i => i.Cashier)
            .Include(i => i.Payment)
            .Where(i => i.InvoiceDate >= today && i.InvoiceDate < tomorrow)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<Invoice?> GetWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(i => i.Items).ThenInclude(ii => ii.Product)
            .Include(i => i.Payment)
            .Include(i => i.Cashier)
            .FirstOrDefaultAsync(i => i.Id == id);
    }
}
