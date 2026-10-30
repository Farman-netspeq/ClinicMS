using ClinicMS.Domain.Enums;

namespace ClinicMS.Application.DTOs.Audit
{
    public class AuditLogListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public AuditAction Action { get; set; }
        public string ActionDisplay => Action.ToString();
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime TransDate { get; set; }
    }
}