using ClinicMS.Application.DTOs.Appointment;
using ClinicMS.Application.DTOs.MedicalRecord;
using ClinicMS.Application.DTOs.Prescription;
using ClinicMS.Shared.Common;
using ClinicMS.Web.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Web.Controllers
{
    [Authorize(Roles = "Admin,Doctor")]
    public class ConsultationController : Controller
    {
        private readonly IHttpService _httpService;
        public ConsultationController(IHttpService httpService) => _httpService = httpService;

        // GET: /Consultation/Index/{appointmentId}
        public async Task<IActionResult> Index(string appointmentId)
        {
            ApiResponseDto<AppointmentResponseDto>? appt;
            try
            {
                appt = await _httpService.GetAsync<ApiResponseDto<AppointmentResponseDto>>(
                    $"api/appointments/{appointmentId}");
            }
            catch (HttpRequestException)
            {
                TempData["MsgCode"] = "error";
                TempData["Msg"] = "Appointment not found";
                return RedirectToAction("Index", "Appointments");
            }

            if (appt?.Data == null)
            {
                TempData["MsgCode"] = "error";
                TempData["Msg"] = "Appointment not found";
                return RedirectToAction("Index", "Appointments");
            }

            ViewBag.Appointment = appt.Data;
            ViewBag.AppointmentId = appointmentId;

            MedicalRecordResponseDto? existingRecord = null;
            PrescriptionResponseDto? existingPrescription = null;

            try
            {
                var recordResult = await _httpService.GetAsync<ApiResponseDto<MedicalRecordResponseDto>>(
                    $"api/medicalrecords/by-appointment/{appointmentId}");
                existingRecord = recordResult?.Data;
            }
            catch (HttpRequestException) {  }

            try
            {
                var prescriptionResult = await _httpService.GetAsync<ApiResponseDto<PrescriptionResponseDto>>(
                    $"api/prescriptions/by-appointment/{appointmentId}");
                existingPrescription = prescriptionResult?.Data;
            }
            catch (HttpRequestException) {  }

            if (existingRecord != null && existingPrescription != null)
            {
                ViewBag.MedicalRecord = existingRecord;
                ViewBag.Prescription = existingPrescription;
                return View("View");
            }

            var dto = new MedicalRecordRequestDto { AppointmentId = appointmentId };
            var rxDto = new PrescriptionRequestDto
            {
                AppointmentId = appointmentId,
                Items = new List<PrescriptionItemDto> { new PrescriptionItemDto() }
            };

            ViewBag.PrescriptionDto = rxDto;
            return View("Index", dto);
        }

        // POST: /Consultation/SaveRecord — saves vitals + diagnosis
        [HttpPost]
        public async Task<IActionResult> SaveRecord(MedicalRecordRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .Select(kvp => kvp.Value!.Errors.First().ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            try
            {
                var result = await _httpService.PostAsync<ApiResponseDto<string>>("api/medicalrecords", dto);
                return Json(new { success = result?.Success ?? false, message = result?.Message ?? "Failed to save" });
            }
            catch (HttpRequestException ex)
            {
                return Json(new { success = false, message = ExtractApiMessage(ex.Message) });
            }
        }

        // POST: /Consultation/SavePrescription — saves prescription + items (master-detail)
        [HttpPost]
        public async Task<IActionResult> SavePrescription([FromBody] PrescriptionRequestDto dto)
        {
            if (!ModelState.IsValid || dto.Items == null || dto.Items.Count == 0)
            {
                return Json(new { success = false, message = "Add at least one valid medication." });
            }

            try
            {
                var result = await _httpService.PostAsync<ApiResponseDto<string>>("api/prescriptions", dto);
                return Json(new
                {
                    success = result?.Success ?? false,
                    message = result?.Message ?? "Failed to save",
                    url = Url.Action("Index", "Consultation", new { appointmentId = dto.AppointmentId })
                });
            }
            catch (HttpRequestException ex)
            {
                return Json(new { success = false, message = ExtractApiMessage(ex.Message) });
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