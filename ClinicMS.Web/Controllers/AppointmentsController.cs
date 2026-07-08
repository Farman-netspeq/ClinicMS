using ClinicMS.Application.DTOs.Appointment;
using ClinicMS.Application.DTOs.Department;
using ClinicMS.Application.DTOs.Patient;
using ClinicMS.Shared.Common;
using ClinicMS.Web.ApiClients;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace ClinicMS.Web.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly IHttpService _httpService;
        public AppointmentsController(IHttpService httpService) => _httpService = httpService;

        // ── INDEX ──────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var filter = new AppointmentFilterDto();
            var result = await _httpService.PostAsync<ApiResponseDto<PaginatedResult<AppointmentListItemDto>>>(
                "api/appointments/list", filter);

            return View(result?.Data ?? new PaginatedResult<AppointmentListItemDto>());
        }

        // ── GRID PARTIAL (called by CustomAjax grid + paging) ──
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> List(AppointmentFilterDto filter)
        {
            var result = await _httpService.PostAsync<ApiResponseDto<PaginatedResult<AppointmentListItemDto>>>(
                "api/appointments/list", filter);

            return PartialView("_List", result?.Data ?? new PaginatedResult<AppointmentListItemDto>());
        }
        // ── ADD/EDIT MODAL ─────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> AddEdit()
        {
            var dto = new AppointmentRequestDto();

            var depts = await _httpService
                .GetAsync<ApiResponseDto<List<DepartmentListItemDto>>>(
                    "/api/departments/active");

            ViewBag.Departments = depts?.Data ?? new List<DepartmentListItemDto>();

            var patients = await _httpService
                .GetAsync<ApiResponseDto<PaginatedResult<PatientListItemDto>>>(
                    "/api/patients?PageNo=1&PageSize=1000&IsActive=true");
            ViewBag.Patients = patients?.Data?.Items ?? new List<PatientListItemDto>();

            return PartialView("_AddEdit", dto);
        }
        // ── SAVE (book) ─────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Save(AppointmentRequestDto dto)
        {
            try
            {
                var result = await _httpService.PostAsync<ApiResponseDto<string>>("api/appointments/book", dto);
                return Json(new
                {
                    success = result?.Success ?? false,
                    message = result?.Message ?? "Booking failed.",
                    url = Url.Action("List")
                });
            }
            catch (HttpRequestException ex)
            {
                return Json(new { success = false, message = ExtractApiMessage(ex.Message), url = "" });
            }
        }

        // ── CANCEL ──────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Cancel(string id, string cancelReason)
        {
            var dto = new CancelAppointmentDto { CancelReason = cancelReason };
            var result = await _httpService.PostAsync<ApiResponseDto<string>>($"api/appointments/{id}/cancel", dto);
            return Json(new { success = result?.Success ?? false, message = result?.Message });
        }

        // ══════════════════════════════════════════════════
        // CASCADING DROPDOWN ENDPOINTS
        // Dept selected → GetDoctorsByDept
        // Doctor + Date selected → GetSlots
        // ══════════════════════════════════════════════════

        // Step 1→2: Department picked → return doctors in that dept
        [HttpGet("/Appointments/GetDoctorsByDepartment/{departmentId}")]
        public async Task<IActionResult> GetDoctorsByDepartment(string departmentId)
        {
            if (string.IsNullOrEmpty(departmentId))
                return Content("[]", "application/json");

            try
            {
                var result = await _httpService.GetAsync<List<DropdownItemDto>>(
                    $"api/doctors/bydepartment/{departmentId}");

                var json = System.Text.Json.JsonSerializer.Serialize(
                    result ?? new List<DropdownItemDto>(),
                    new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null });

                return Content(json, "application/json");
            }
            catch (HttpRequestException)
            {
                return Content("[]", "application/json");
            }
        }

        // Step 2→3: Doctor + Date picked → return available slots
        [HttpGet]
        public async Task<IActionResult> GetSlots(string doctorId, DateTime date)
        {
            if (string.IsNullOrEmpty(doctorId))
                return Content("[]", "application/json");

            try
            {
                var result = await _httpService.GetAsync<ApiResponseDto<List<AppointmentSlotDto>>>(
                    $"api/appointments/slots?doctorId={doctorId}&date={date:yyyy-MM-dd}");

                if (result?.Data == null || !result.Data.Any())
                    return Content("[]", "application/json");

                var options = result.Data.Select(s => new
                {
                    Value = $"{s.StartTime}|{s.EndTime}",
                    Text = $"{s.StartTime} - {s.EndTime}"
                }).ToList();

                var json = System.Text.Json.JsonSerializer.Serialize(options,
                    new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null });

                return Content(json, "application/json");
            }
            catch (HttpRequestException)
            {
                // Api rejected (e.g. "Doctor is not available") — return empty, not a crash
                return Content("[]", "application/json");
            }
        }
        private string ExtractApiMessage(string exceptionMessage)
        {
            try
            {
                var jsonStart = exceptionMessage.IndexOf('{');
                if (jsonStart >= 0)
                {
                    var json = exceptionMessage.Substring(jsonStart);
                    var doc = System.Text.Json.JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("message", out var msg))
                        return msg.GetString() ?? "Save failed.";
                }
            }
            catch { }
            return "Save failed.";
        }
    }
}