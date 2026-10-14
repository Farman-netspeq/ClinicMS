using ClinicMS.Application.DTOs.Invoice;
using ClinicMS.Shared.Common;

namespace ClinicMS.Application.Interfaces
{
    public interface IInvoiceService
    {
        Task<Result<PaginatedResult<InvoiceListItemDto>>> GetInvoicesAsync(InvoiceFilterDto filter);
        Task<Result<InvoiceResponseDto>> GetByIdAsync(string id);
        Task<Result<InvoiceResponseDto>> GetByAppointmentIdAsync(string appointmentId);
        Task<Result<string>> GenerateAsync(InvoiceRequestDto dto, string userId);
        Task<Result> PayAsync(string id, InvoicePayDto dto, string userId);
    }
}