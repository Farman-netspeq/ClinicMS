using System.Data;
using System.Text.Json;
using ClinicMS.Application.DTOs.Invoice;
using ClinicMS.Application.Interfaces;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.Data.SqlClient;
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
                var searchTermParam = new SqlParameter("@SearchTerm", (object?)filter.SearchTerm ?? DBNull.Value);
                var statusParam = new SqlParameter("@Status", (object?)(int?)filter.Status ?? DBNull.Value);
                var fromDateParam = new SqlParameter("@FromDate", (object?)filter.FromDate ?? DBNull.Value);
                var toDateParam = new SqlParameter("@ToDate", (object?)filter.ToDate ?? DBNull.Value);
                var pageNoParam = new SqlParameter("@PageNo", filter.PageNo);
                var pageSizeParam = new SqlParameter("@PageSize", filter.PageSize);

                var items = await _context.InvoiceListItems
                    .FromSqlRaw("EXEC udspInvoicesPaged @SearchTerm, @Status, @FromDate, @ToDate, @PageNo, @PageSize",
                        searchTermParam, statusParam, fromDateParam, toDateParam, pageNoParam, pageSizeParam)
                    .ToListAsync();

                var countSearchTermParam = new SqlParameter("@SearchTerm", (object?)filter.SearchTerm ?? DBNull.Value);
                var countStatusParam = new SqlParameter("@Status", (object?)(int?)filter.Status ?? DBNull.Value);
                var countFromDateParam = new SqlParameter("@FromDate", (object?)filter.FromDate ?? DBNull.Value);
                var countToDateParam = new SqlParameter("@ToDate", (object?)filter.ToDate ?? DBNull.Value);

                var countResult = (await _context.InvoiceCounts
      .FromSqlRaw("EXEC udspInvoicesPagedCount @SearchTerm, @Status, @FromDate, @ToDate",
          countSearchTermParam, countStatusParam, countFromDateParam, countToDateParam)
      .ToListAsync())
      .FirstOrDefault();
                var totalCount = countResult?.TotalCount ?? 0;

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
                var idParam = new SqlParameter("@Id", id);

                var header = (await _context.InvoiceHeaders
                    .FromSqlRaw("EXEC udspInvoicesGetById @Id", idParam)
                    .ToListAsync())
                    .FirstOrDefault();
                if (header == null)
                    return Result<InvoiceResponseDto>.Fail("Invoice not found");

                var invoiceIdParam = new SqlParameter("@InvoiceId", header.Id);

                var items = await _context.InvoiceItemResults
                    .FromSqlRaw("EXEC udspInvoiceItemsGetByInvoiceId @InvoiceId", invoiceIdParam)
                    .ToListAsync();

                return Result<InvoiceResponseDto>.Ok(MapToResponse(header, items));
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
                var appointmentIdParam = new SqlParameter("@AppointmentId", appointmentId);

                var header = (await _context.InvoiceHeaders
     .FromSqlRaw("EXEC udspInvoicesGetByAppointmentId @AppointmentId", appointmentIdParam)
     .ToListAsync())
     .FirstOrDefault();

                if (header == null)
                    return Result<InvoiceResponseDto>.Fail("No invoice exists for this appointment");

                var invoiceIdParam = new SqlParameter("@InvoiceId", header.Id);

                var items = await _context.InvoiceItemResults
                    .FromSqlRaw("EXEC udspInvoiceItemsGetByInvoiceId @InvoiceId", invoiceIdParam)
                    .ToListAsync();

                return Result<InvoiceResponseDto>.Ok(MapToResponse(header, items));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoice for appointment {AppointmentId}", appointmentId);
                return Result<InvoiceResponseDto>.Fail("Failed to fetch invoice");
            }
        }

        public async Task<Result<string>> GenerateAsync(InvoiceRequestDto dto)
        {
            try
            {
                var id = Guid.NewGuid().ToString();
                var itemsJson = JsonSerializer.Serialize(dto.Items);

                var idParam = new SqlParameter("@Id", id);
                var appointmentIdParam = new SqlParameter("@AppointmentId", dto.AppointmentId);
                var itemsJsonParam = new SqlParameter("@ItemsJson", itemsJson);
                var newInvoiceNumberParam = new SqlParameter
                {
                    ParameterName = "@NewInvoiceNumber",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 20,
                    Direction = ParameterDirection.Output
                };
                var resultParam = new SqlParameter
                {
                    ParameterName = "@Result",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 200,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspInvoicesGenerate @Id, @AppointmentId, @ItemsJson, @NewInvoiceNumber OUTPUT, @Result OUTPUT",
                    idParam, appointmentIdParam, itemsJsonParam, newInvoiceNumberParam, resultParam);

                var errorMessage = resultParam.Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(errorMessage))
                    return Result<string>.Fail(errorMessage);

                var invoiceNumber = newInvoiceNumberParam.Value?.ToString() ?? string.Empty;

                _logger.LogInformation("Invoice generated: {Id} - {Number} for appointment {AppointmentId}",
                    id, invoiceNumber, dto.AppointmentId);

                return Result<string>.Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice: {@Dto}", dto);
                return Result<string>.Fail("Failed to generate invoice");
            }
        }

        public async Task<Result> PayAsync(string id, InvoicePayDto dto)
        {
            try
            {
                var idParam = new SqlParameter("@Id", id);
                var paymentMethodParam = new SqlParameter("@PaymentMethod", (int)dto.PaymentMethod);
                var resultParam = new SqlParameter
                {
                    ParameterName = "@Result",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 200,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspInvoicesPay @Id, @PaymentMethod, @Result OUTPUT",
                    idParam, paymentMethodParam, resultParam);

                var errorMessage = resultParam.Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(errorMessage))
                    return Result.Fail(errorMessage);

                _logger.LogInformation("Invoice paid: {Id} via {Method}", id, dto.PaymentMethod);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error paying invoice {Id}", id);
                return Result.Fail("Failed to process payment");
            }
        }

        private static InvoiceResponseDto MapToResponse(InvoiceHeaderDto header, List<InvoiceItemDto> items)
        {
            return new InvoiceResponseDto
            {
                Id = header.Id,
                InvoiceNumber = header.InvoiceNumber,
                PatientId = header.PatientId,
                PatientName = header.PatientName,
                AppointmentId = header.AppointmentId,
                IssuedOn = header.IssuedOn,
                Status = header.Status,
                TotalAmount = header.TotalAmount,
                PaidOn = header.PaidOn,
                PaymentMethod = header.PaymentMethod,
                Items = items
            };
        }
    }
}