using ClinicMS.Application.DTOs.Audit;
using ClinicMS.Domain.Enums;
using ClinicMS.Shared.Common;

namespace ClinicMS.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string userId, string userName, AuditAction action, string entityType, string entityId, string? details = null);
        Task<Result<PaginatedResult<AuditLogListItemDto>>> GetAuditLogsAsync(AuditLogFilterDto filter);
    }
}