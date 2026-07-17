using ClinicMS.Application.DTOs.Invoice;
using ClinicMS.Shared.Common;
using ClinicMS.Web.ApiClients;
using ClinicMS.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Web.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class InvoicesController : Controller
    {
        private readonly IHttpService _httpService;
        public InvoicesController(IHttpService httpService) => _httpService = httpService;

        public async Task<IActionResult> Index()
        {
            var filter = new InvoiceFilterDto();
            var result = await _httpService.GetAsync<ApiResponseDto<PaginatedResult<InvoiceListItemDto>>>(
                $"api/invoices?PageNo={filter.PageNo}&PageSize={filter.PageSize}");

            return View(result?.Data ?? new PaginatedResult<InvoiceListItemDto>());
        }
        // ── GRID PARTIAL ──
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] InvoiceFilterDto filter)
        {
            var query = $"api/invoices?PageNo={filter.PageNo}&PageSize={filter.PageSize}";
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query += $"&SearchTerm={Uri.EscapeDataString(filter.SearchTerm)}";
            if (filter.Status.HasValue)
                query += $"&Status={filter.Status.Value}";
            if (filter.FromDate.HasValue)
                query += $"&FromDate={filter.FromDate.Value:yyyy-MM-dd}";
            if (filter.ToDate.HasValue)
                query += $"&ToDate={filter.ToDate.Value:yyyy-MM-dd}";

            var result = await _httpService.GetAsync<ApiResponseDto<PaginatedResult<InvoiceListItemDto>>>(query);
            return PartialView("_List", result?.Data ?? new PaginatedResult<InvoiceListItemDto>());
        }

        // ── GENERATE FORM (from a Completed appointment) ──
        [HttpGet]
        public async Task<IActionResult> Generate(string appointmentId)
        {
            var appt = await _httpService.GetAsync<ApiResponseDto<ClinicMS.Application.DTOs.Appointment.AppointmentResponseDto>>(
                $"api/appointments/{appointmentId}");

            if (appt?.Data == null)
            {
                TempData["MsgCode"] = "error";
                TempData["Msg"] = "Appointment not found";
                return RedirectToAction("Index", "Appointments");
            }

            // BR6: an invoice already exists for this appointment —
            try
            {
                var existing = await _httpService.GetAsync<ApiResponseDto<InvoiceResponseDto>>(
                    $"api/invoices/by-appointment/{appointmentId}");
                if (existing?.Data != null)
                    return RedirectToAction("Details", new { id = existing.Data.Id });
            }
            catch (HttpRequestException)
            {
                // 404 from Api = no invoice yet for this appointment, this is the expected/normal path — fall through
            }

            ViewBag.Appointment = appt.Data;

            var dto = new InvoiceRequestDto
            {
                AppointmentId = appointmentId,
                Items = new List<InvoiceItemDto>
        {
            new InvoiceItemDto
            {
                Description = "Consultation Fee",
                Quantity = 1,
                UnitPrice = appt.Data.ConsultationFee
            }
        }
            };

            return View(dto);
        }
        // ── SAVE (generate) ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate([FromBody] InvoiceRequestDto dto)
        {
            try
            {
                var result = await _httpService.PostAsync<ApiResponseDto<string>>("api/invoices", dto);
                return Json(new
                {
                    success = result?.Success ?? false,
                    message = result?.Message ?? "Failed to generate invoice.",
                    url = result?.Success == true ? Url.Action("Details", new { id = result.Data }) : ""
                });
            }
            catch (HttpRequestException ex)
            {
                return Json(new { success = false, message = ApiErrorHelper.ExtractApiMessage(ex.Message) });
            }
        }

        // ── DETAILS (view + pay action) ──
        public async Task<IActionResult> Details(string id)
        {
            var result = await _httpService.GetAsync<ApiResponseDto<InvoiceResponseDto>>($"api/invoices/{id}");
            if (result?.Data == null)
            {
                TempData["MsgCode"] = "error";
                TempData["Msg"] = "Invoice not found";
                return RedirectToAction("Index");
            }
            return View(result.Data);
        }

        // ── PAY ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(string id, InvoicePayDto dto)
        {
            try
            {
                var result = await _httpService.PostAsync<ApiResponseDto<string>>($"api/invoices/{id}/pay", dto);
                return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Payment failed." });
            }
            catch (HttpRequestException ex)
            {
                return Json(new { success = false, message = ApiErrorHelper.ExtractApiMessage(ex.Message) });
            }
        }
    }
}