using ClinicMS.Application.DTOs.Invoice;
using ClinicMS.Application.Interfaces;
using ClinicMS.Domain.Entities;
using ClinicMS.Domain.Enums;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicMS.Infrastructure.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(ApplicationDbContext context, ILogger<InvoiceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<PaginatedResult<InvoiceListItemDto>>> GetInvoicesAsync(InvoiceFilterDto filter)
        {
            try
            {
                var query = _context.Invoices
                    .Include(i => i.Patient)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    query = query.Where(i =>
                        i.InvoiceNumber.Contains(filter.SearchTerm) ||
                        i.Patient!.FirstName.Contains(filter.SearchTerm) ||
                        i.Patient!.LastName.Contains(filter.SearchTerm) ||
                        i.Patient!.PatientNumber.Contains(filter.SearchTerm));

                if (filter.Status.HasValue)
                    query = query.Where(i => i.Status == filter.Status.Value);

                if (filter.FromDate.HasValue)
                    query = query.Where(i => i.IssuedOn >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(i => i.IssuedOn <= filter.ToDate.Value);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(i => i.IssuedOn)
                    .Skip((filter.PageNo - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(i => new InvoiceListItemDto
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        PatientName = i.Patient!.FirstName + " " + i.Patient.LastName,
                        PatientNumber = i.Patient.PatientNumber,
                        IssuedOn = i.IssuedOn,
                        Status = i.Status.ToString(),
                        TotalAmount = i.TotalAmount
                    })
                    .ToListAsync();

                return Result<PaginatedResult<InvoiceListItemDto>>.Ok(new PaginatedResult<InvoiceListItemDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = filter.PageNo,
                    PageSize = filter.PageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoices");
                return Result<PaginatedResult<InvoiceListItemDto>>.Fail("Failed to fetch invoices");
            }
        }

        public async Task<Result<InvoiceResponseDto>> GetByIdAsync(string id)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Patient)
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null)
                    return Result<InvoiceResponseDto>.Fail("Invoice not found");

                return Result<InvoiceResponseDto>.Ok(new InvoiceResponseDto
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    PatientId = invoice.PatientId,
                    PatientName = $"{invoice.Patient?.FirstName} {invoice.Patient?.LastName}",
                    AppointmentId = invoice.AppointmentId,
                    IssuedOn = invoice.IssuedOn,
                    Status = invoice.Status,
                    TotalAmount = invoice.TotalAmount,
                    PaidOn = invoice.PaidOn,
                    PaymentMethod = invoice.PaymentMethod,
                    Items = invoice.Items.Select(it => new InvoiceItemDto
                    {
                        Id = it.Id,
                        Description = it.Description,
                        Quantity = it.Quantity,
                        UnitPrice = it.UnitPrice
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoice {Id}", id);
                return Result<InvoiceResponseDto>.Fail("Failed to fetch invoice");
            }
        }

        public async Task<Result<InvoiceResponseDto>> GetByAppointmentIdAsync(string appointmentId)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Patient)
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);

                if (invoice == null)
                    return Result<InvoiceResponseDto>.Fail("No invoice exists for this appointment");

                return Result<InvoiceResponseDto>.Ok(new InvoiceResponseDto
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    PatientId = invoice.PatientId,
                    PatientName = $"{invoice.Patient?.FirstName} {invoice.Patient?.LastName}",
                    AppointmentId = invoice.AppointmentId,
                    IssuedOn = invoice.IssuedOn,
                    Status = invoice.Status,
                    TotalAmount = invoice.TotalAmount,
                    PaidOn = invoice.PaidOn,
                    PaymentMethod = invoice.PaymentMethod,
                    Items = invoice.Items.Select(it => new InvoiceItemDto
                    {
                        Id = it.Id,
                        Description = it.Description,
                        Quantity = it.Quantity,
                        UnitPrice = it.UnitPrice
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoice for appointment {AppointmentId}", appointmentId);
                return Result<InvoiceResponseDto>.Fail("Failed to fetch invoice");
            }
        }

        // ── BR6: generate only from Completed appointment, once per appointment ──
        public async Task<Result<string>> GenerateAsync(InvoiceRequestDto dto)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

                if (appointment == null)
                    return Result<string>.Fail("Appointment not found");

                if (appointment.Status != AppointmentStatus.Completed)
                    return Result<string>.Fail("Invoice can only be generated for a completed appointment");

                var exists = await _context.Invoices.AnyAsync(i => i.AppointmentId == dto.AppointmentId);
                if (exists)
                    return Result<string>.Fail("An invoice already exists for this appointment");

                if (dto.Items == null || dto.Items.Count == 0)
                    return Result<string>.Fail("At least one line item is required");

                // BR1: auto invoice number
                var invoiceNumber = await GenerateInvoiceNumberAsync();

                var items = dto.Items.Select(i => new InvoiceItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Description = i.Description.Trim(),
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice   // computed server-side
                }).ToList();

                var totalAmount = items.Sum(i => i.LineTotal);

                var invoice = new Invoice
                {
                    Id = Guid.NewGuid().ToString(),
                    InvoiceNumber = invoiceNumber,
                    PatientId = appointment.PatientId,
                    AppointmentId = appointment.Id,
                    IssuedOn = DateTime.UtcNow,
                    Status = InvoiceStatus.Pending,
                    TotalAmount = totalAmount,
                    Items = items
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Invoice generated: {Id} - {Number} for appointment {AppointmentId}, total {Total}",
                    invoice.Id, invoiceNumber, appointment.Id, totalAmount);

                return Result<string>.Ok(invoice.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice: {@Dto}", dto);
                return Result<string>.Fail("Failed to generate invoice");
            }
        }

        // ── BR6: pay + immutability ──
        public async Task<Result> PayAsync(string id, InvoicePayDto dto)
        {
            try
            {
                var invoice = await _context.Invoices.FindAsync(id);
                if (invoice == null)
                    return Result.Fail("Invoice not found");

                if (invoice.Status == InvoiceStatus.Paid)
                    return Result.Fail("Invoice is already paid");

                if (invoice.Status == InvoiceStatus.Cancelled)
                    return Result.Fail("Cannot pay a cancelled invoice");

                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidOn = DateTime.UtcNow;
                invoice.PaymentMethod = dto.PaymentMethod;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Invoice paid: {Id} via {Method}", id, dto.PaymentMethod);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error paying invoice {Id}", id);
                return Result.Fail("Failed to process payment");
            }
        }

        // ── BR1 helper ──
        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var last = await _context.Invoices
                .OrderByDescending(i => i.InvoiceNumber)
                .Select(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var parts = last.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int lastNumber))
                    nextNumber = lastNumber + 1;
            }
            return $"INV-{nextNumber:D6}";
        }
    }
}