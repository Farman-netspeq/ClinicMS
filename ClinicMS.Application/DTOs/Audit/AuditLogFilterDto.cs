using ClinicMS.Domain.Enums;

namespace ClinicMS.Application.DTOs.Audit
{
    public class AuditLogFilterDto
    {
        public string? EntityType { get; set; }
        public AuditAction? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}