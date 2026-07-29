using ClinicMS.Domain.Enums;

namespace ClinicMS.Domain.Entities
{
    public class AuditLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public AuditAction Action { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime TransDate { get; set; } = DateTime.UtcNow;

        public ApplicationUser? User { get; set; }
    }
}