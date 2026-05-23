using POSDesktopSystem.Domain.Entities;
using POSDesktopSystem.Domain.Interfaces.Repositories;
using POSDesktopSystem.Infrastructure.Data;

namespace POSDesktopSystem.Infrastructure.Repositories;

public class InvoiceItemRepository : Repository<InvoiceItem>, IInvoiceItemRepository
{
    public InvoiceItemRepository(AppDbContext context) : base(context) { }
}
