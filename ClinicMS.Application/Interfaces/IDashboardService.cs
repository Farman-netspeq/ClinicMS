using ClinicMS.Application.DTOs.Dashboard;
using ClinicMS.Shared.Common;

namespace ClinicMS.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<Result<DashboardSummaryDto>> GetSummaryAsync(string? doctorUserId);
    }
}