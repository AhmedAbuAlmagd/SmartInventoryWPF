using POSDesktopSystem.Application.DTOs.Common;
using POSDesktopSystem.Application.DTOs.Invoices;
using POSDesktopSystem.Application.Exceptions;
using POSDesktopSystem.Application.Interfaces;
using POSDesktopSystem.Domain.Entities;
using POSDesktopSystem.Domain.Enums;
using POSDesktopSystem.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace POSDesktopSystem.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(IUnitOfWork unitOfWork, ILogger<InvoiceService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<InvoiceDto> GetByIdAsync(int id)
    {
        var invoice = await _unitOfWork.Invoices.GetWithDetailsAsync(id);
        if (invoice == null)
            throw new NotFoundException($"Invoice with ID {id} not found.");
        return MapToDto(invoice);
    }

    public async Task<PagedResultDto<InvoiceDto>> GetAllAsync(int page, int pageSize, InvoiceStatus? status)
    {
        var (items, totalCount) = await _unitOfWork.Invoices.GetPagedAsync(page, pageSize, status);
        return new PagedResultDto<InvoiceDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            throw new ValidationException("Invoice must have at least one item.");

        var invoice = new Invoice
        {
            InvoiceNumber = await _unitOfWork.Invoices.GenerateInvoiceNumberAsync(),
            InvoiceDate = DateTime.UtcNow,
            Status = InvoiceStatus.Paid,
            CashierId = dto.CashierId,
            DiscountAmount = dto.DiscountAmount,
            TaxRate = dto.TaxRate,
            CreatedAt = DateTime.UtcNow
        };

        decimal subTotal = 0;
        foreach (var itemDto in dto.Items)
        {
            if (itemDto.Quantity < 1)
                throw new ValidationException("Quantity must be at least 1.");

            var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
            if (product == null || !product.IsActive)
                throw new ValidationException($"Product {itemDto.ProductName} is not available.");

            if (product.StockQuantity < itemDto.Quantity)
                throw new InsufficientStockException($"Insufficient stock for {product.Name}. Available: {product.StockQuantity}");

            product.StockQuantity -= itemDto.Quantity;
            _unitOfWork.Products.Update(product);

            var lineTotal = product.Price * itemDto.Quantity;
            subTotal += lineTotal;

            invoice.Items.Add(new InvoiceItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = itemDto.Quantity,
                LineTotal = lineTotal,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (dto.DiscountAmount < 0 || dto.DiscountAmount > subTotal)
            throw new ValidationException("Invalid discount amount.");

        decimal taxAmount = (subTotal - dto.DiscountAmount) * dto.TaxRate;
        decimal totalAmount = subTotal - dto.DiscountAmount + taxAmount;

        invoice.SubTotal = subTotal;
        invoice.TaxAmount = taxAmount;
        invoice.TotalAmount = totalAmount;

        if (dto.PaymentMethod == PaymentMethod.Cash && dto.AmountPaid < totalAmount)
            throw new ValidationException("Amount paid is less than total amount for Cash payment.");
        else if (dto.PaymentMethod == PaymentMethod.Card)
            dto.AmountPaid = totalAmount; // Force equal for card
            
        decimal change = dto.PaymentMethod == PaymentMethod.Cash ? dto.AmountPaid - totalAmount : 0;

        invoice.Payment = new Payment
        {
            Method = dto.PaymentMethod,
            AmountPaid = dto.AmountPaid,
            Change = change,
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Invoices.AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Sale completed: Invoice {InvoiceNumber} (Total: {TotalAmount}, Payment: {PaymentMethod})", 
            invoice.InvoiceNumber, invoice.TotalAmount, invoice.Payment.Method);

        var savedInvoice = await _unitOfWork.Invoices.GetWithDetailsAsync(invoice.Id);
        return MapToDto(savedInvoice!);
    }

    public async Task<InvoiceDto> CancelAsync(int id)
    {
        var invoice = await _unitOfWork.Invoices.GetWithDetailsAsync(id);
        if (invoice == null)
            throw new NotFoundException($"Invoice with ID {id} not found.");

        if (invoice.Status != InvoiceStatus.Open && invoice.Status != InvoiceStatus.Paid)
            throw new ValidationException("Only open or paid invoices can be cancelled.");

        foreach (var item in invoice.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
                _unitOfWork.Products.Update(product);
            }
        }

        invoice.Status = InvoiceStatus.Cancelled;
        invoice.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Invoices.Update(invoice);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning("Invoice cancelled: {InvoiceNumber} (Total: {TotalAmount})", 
            invoice.InvoiceNumber, invoice.TotalAmount);

        return MapToDto(invoice);
    }

    public async Task<IEnumerable<InvoiceDto>> GetTodaysInvoicesAsync()
    {
        var invoices = await _unitOfWork.Invoices.GetTodaysInvoicesAsync();
        return invoices.Select(MapToDto);
    }

    private static InvoiceDto MapToDto(Invoice i) => new()
    {
        Id = i.Id,
        InvoiceNumber = i.InvoiceNumber,
        InvoiceDate = i.InvoiceDate,
        Status = i.Status.ToString(),
        SubTotal = i.SubTotal,
        DiscountAmount = i.DiscountAmount,
        TaxRate = i.TaxRate,
        TaxAmount = i.TaxAmount,
        TotalAmount = i.TotalAmount,
        CashierName = i.Cashier?.Username ?? "Unknown",
        Items = i.Items.Select(it => new InvoiceItemDto
        {
            ProductId = it.ProductId,
            ProductName = it.ProductName,
            Quantity = it.Quantity,
            UnitPrice = it.UnitPrice,
            LineTotal = it.LineTotal
        }).ToList(),
        PaymentMethod = i.Payment?.Method.ToString(),
        AmountPaid = i.Payment?.AmountPaid,
        Change = i.Payment?.Change,
        Payment = i.Payment != null ? new InvoicePaymentDto
        {
            Method = i.Payment.Method.ToString(),
            AmountPaid = i.Payment.AmountPaid,
            Change = i.Payment.Change
        } : null
    };
}
