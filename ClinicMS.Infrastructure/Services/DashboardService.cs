using ClinicMS.Application.DTOs.Dashboard;
using ClinicMS.Application.Interfaces;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ClinicMS.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(ApplicationDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<DashboardSummaryDto>> GetSummaryAsync(string? doctorUserId)
        {
            try
            {
                string? doctorId = null;

                if (!string.IsNullOrEmpty(doctorUserId))
                {
                    var userIdParam = new SqlParameter("@ApplicationUserId", doctorUserId);
                    var doctorIdOutParam = new SqlParameter
                    {
                        ParameterName = "@DoctorId",
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 36,
                        Direction = ParameterDirection.Output
                    };

                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC udspApptGetDoctorIdByUser @ApplicationUserId, @DoctorId OUTPUT",
                        userIdParam, doctorIdOutParam);

                    doctorId = doctorIdOutParam.Value?.ToString();
                    if (string.IsNullOrEmpty(doctorId))
                        return Result<DashboardSummaryDto>.Fail("No doctor profile found for this user");
                }

                var doctorFilterParam = new SqlParameter("@DoctorId", (object?)doctorId ?? DBNull.Value);

                var results = await _context.Set<DashboardSummaryDto>()
                    .FromSqlRaw("EXEC udspDashboardSummary @DoctorId", doctorFilterParam)
                    .ToListAsync();

                var dto = results.FirstOrDefault();
                if (dto == null)
                    return Result<DashboardSummaryDto>.Fail("Failed to load dashboard summary");

                return Result<DashboardSummaryDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard summary for user {UserId}", doctorUserId);
                return Result<DashboardSummaryDto>.Fail("Failed to fetch dashboard summary");
            }
        }
    }
}