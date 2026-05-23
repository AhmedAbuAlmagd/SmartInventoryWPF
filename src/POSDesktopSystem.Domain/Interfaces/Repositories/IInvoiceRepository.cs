using POSDesktopSystem.Domain.Entities;
using POSDesktopSystem.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POSDesktopSystem.Domain.Interfaces.Repositories;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetWithDetailsAsync(int id);       // includes Items + Payment + Cashier
    Task<IEnumerable<Invoice>> GetTodaysInvoicesAsync();
    Task<(IEnumerable<Invoice> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, InvoiceStatus? status);
    Task<string> GenerateInvoiceNumberAsync();        // INV-YYYYMMDD-XXXX format
}
