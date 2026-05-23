using POSDesktopSystem.Application.DTOs.Common;
using POSDesktopSystem.Application.DTOs.Invoices;
using POSDesktopSystem.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POSDesktopSystem.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto> GetByIdAsync(int id);
    Task<PagedResultDto<InvoiceDto>> GetAllAsync(int page, int pageSize, InvoiceStatus? status);
    Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto);
    Task<InvoiceDto> CancelAsync(int id);
    Task<IEnumerable<InvoiceDto>> GetTodaysInvoicesAsync();
}
