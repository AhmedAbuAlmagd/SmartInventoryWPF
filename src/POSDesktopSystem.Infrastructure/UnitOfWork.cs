using POSDesktopSystem.Domain.Interfaces;
using POSDesktopSystem.Domain.Interfaces.Repositories;
using POSDesktopSystem.Infrastructure.Data;
using POSDesktopSystem.Infrastructure.Repositories;
using System.Threading.Tasks;

namespace POSDesktopSystem.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IProductRepository Products { get; }
    public IInvoiceRepository Invoices { get; }
    public IUserRepository Users { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Products = new ProductRepository(_context);
        Invoices = new InvoiceRepository(_context);
        Users = new UserRepository(_context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
