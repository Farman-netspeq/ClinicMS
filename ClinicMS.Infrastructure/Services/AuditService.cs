using ClinicMS.Application.DTOs.Audit;
using ClinicMS.Application.Interfaces;
using ClinicMS.Domain.Entities;
using ClinicMS.Domain.Enums;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicMS.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(string userId, string userName, AuditAction action, string entityType, string entityId, string? details = null)
        {
            try
            {
                var log = new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Details = details,
                    TransDate = DateTime.UtcNow
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // audit failure must never break the calling action
                _logger.LogError(ex, "Failed to write audit log for {EntityType} {EntityId}", entityType, entityId);
            }
        }

        public async Task<Result<PaginatedResult<AuditLogListItemDto>>> GetAuditLogsAsync(AuditLogFilterDto filter)
        {
            try
            {
                var entityTypeParam = new SqlParameter("@EntityType", (object?)filter.EntityType ?? DBNull.Value);
                var actionParam = new SqlParameter("@Action", (object?)(int?)filter.Action ?? DBNull.Value);
                var fromDateParam = new SqlParameter("@FromDate", (object?)filter.FromDate ?? DBNull.Value);
                var toDateParam = new SqlParameter("@ToDate", (object?)filter.ToDate ?? DBNull.Value);
                var pageNoParam = new SqlParameter("@PageNo", filter.PageNo);
                var pageSizeParam = new SqlParameter("@PageSize", filter.PageSize);

                var items = await _context.Set<AuditLogListItemDto>()
                    .FromSqlRaw(
                        "EXEC udspAuditLogsPaged @EntityType, @Action, @FromDate, @ToDate, @PageNo, @PageSize",
                        entityTypeParam, actionParam, fromDateParam, toDateParam, pageNoParam, pageSizeParam)
                    .ToListAsync();

                var countEntityTypeParam = new SqlParameter("@EntityType", (object?)filter.EntityType ?? DBNull.Value);
                var countActionParam = new SqlParameter("@Action", (object?)(int?)filter.Action ?? DBNull.Value);
                var countFromDateParam = new SqlParameter("@FromDate", (object?)filter.FromDate ?? DBNull.Value);
                var countToDateParam = new SqlParameter("@ToDate", (object?)filter.ToDate ?? DBNull.Value);

                var countResult = await _context.Set<AuditLogCountResultDto>()
                    .FromSqlRaw(
                        "EXEC udspAuditLogsPagedCount @EntityType, @Action, @FromDate, @ToDate",
                        countEntityTypeParam, countActionParam, countFromDateParam, countToDateParam)
                    .ToListAsync();

                var totalCount = countResult.FirstOrDefault()?.TotalCount ?? 0;

                return Result<PaginatedResult<AuditLogListItemDto>>.Ok(new PaginatedResult<AuditLogListItemDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = filter.PageNo,
                    PageSize = filter.PageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching audit logs. Filter: {@Filter}", filter);
                return Result<PaginatedResult<AuditLogListItemDto>>.Fail("Failed to fetch audit logs");
            }
        }
    }
}