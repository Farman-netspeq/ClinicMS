using ClinicMS.Application.DTOs.Audit;
using ClinicMS.Shared.Common;
using ClinicMS.Web.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : Controller
    {
        private readonly IHttpService _httpService;

        public AuditLogsController(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await LoadGridAsync(new AuditLogFilterDto());
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Grid(int pageNo = 1, int pageSize = 10, string? entityType = null,
         int? auditAction = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var filter = new AuditLogFilterDto
            {
                PageNo = pageNo,
                PageSize = pageSize,
                EntityType = entityType,
                Action = auditAction.HasValue ? (ClinicMS.Domain.Enums.AuditAction)auditAction.Value : null,
                FromDate = fromDate,
                ToDate = toDate
            };
            var data = await LoadGridAsync(filter);
            return PartialView("_Grid", data);
        }

        private async Task<PaginatedResult<AuditLogListItemDto>> LoadGridAsync(AuditLogFilterDto filter)
        {
            var response = await _httpService.PostAsync<ApiResponseDto<PaginatedResult<AuditLogListItemDto>>>(
                "api/auditlogs/list", filter);
            return response!.Data!;
        }
    }
}