using ClinicMS.Domain.Enums;

namespace ClinicMS.Application.DTOs.Invoice
{
    public class InvoiceFilterDto
    {
        public string? SearchTerm { get; set; }     
        public InvoiceStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}