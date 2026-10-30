using ClinicMS.Application.DTOs.Dashboard;
using ClinicMS.Shared.Common;
using ClinicMS.Web.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IHttpService _httpService;
        public DashboardController(IHttpService httpService) => _httpService = httpService;

        public async Task<IActionResult> Index()
        {
            var result = await _httpService.GetAsync<ApiResponseDto<DashboardSummaryDto>>("api/dashboard/summary");
            return View(result?.Data ?? new DashboardSummaryDto());
        }
    }
}