using ClinicMS.Application.DTOs.Dashboard;
using ClinicMS.Application.Interfaces;
using ClinicMS.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClinicMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;
        public DashboardController(IDashboardService service) => _service = service;

        [HttpGet("summary", Name = "GetDashboardSummary")]
        public async Task<IActionResult> GetSummary()
        {
            // Doctor → scoped (BR10); Admin/Receptionist → clinic-wide (null)
            string? doctorUserId = User.IsInRole("Doctor")
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            var result = await _service.GetSummaryAsync(doctorUserId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<DashboardSummaryDto>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<DashboardSummaryDto>.SuccessResponse(result.Data!));
        }
    }
}