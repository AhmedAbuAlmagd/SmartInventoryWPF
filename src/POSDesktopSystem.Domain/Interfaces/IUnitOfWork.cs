using POSDesktopSystem.Domain.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace POSDesktopSystem.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IInvoiceRepository Invoices { get; }
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync();
}
