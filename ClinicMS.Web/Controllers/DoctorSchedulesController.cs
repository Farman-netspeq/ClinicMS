using ClinicMS.Application.DTOs.DoctorSchedule;
using ClinicMS.Shared.Common;
using ClinicMS.Web.ApiClients;
using Microsoft.AspNetCore.Mvc;

public class DoctorSchedulesController : Controller
{
    private readonly IHttpService _httpService;
    public DoctorSchedulesController(IHttpService httpService) => _httpService = httpService;

    public async Task<IActionResult> Index(string doctorId)
    {
        var result = await _httpService.GetAsync<ApiResponseDto<List<DoctorScheduleListItemDto>>>(
            $"api/doctorschedules/by-doctor/{doctorId}");
        ViewBag.DoctorId = doctorId;
        return View(result?.Data ?? new List<DoctorScheduleListItemDto>());
    }

    [HttpGet]
    public async Task<IActionResult> List(string doctorId)
    {
        var result = await _httpService.GetAsync<ApiResponseDto<List<DoctorScheduleListItemDto>>>(
            $"api/doctorschedules/by-doctor/{doctorId}");
        ViewBag.DoctorId = doctorId;
        return PartialView("_List", result?.Data ?? new List<DoctorScheduleListItemDto>());
    }

    [HttpGet]
    public async Task<IActionResult> AddEdit(string id, string doctorId)
    {
        var dto = new DoctorScheduleRequestDto { DoctorId = doctorId };
        if (!string.IsNullOrEmpty(id) && id != "0")
        {
            var result = await _httpService.GetAsync<ApiResponseDto<DoctorScheduleResponseDto>>($"api/doctorschedules/{id}");
            if (result?.Data != null)
            {
                dto.Id = result.Data.Id;
                dto.DoctorId = result.Data.DoctorId;
                dto.DayOfWeek = result.Data.DayOfWeek;
                dto.StartTime = result.Data.StartTime;
                dto.EndTime = result.Data.EndTime;
                dto.SlotDurationMinutes = result.Data.SlotDurationMinutes;
            }
        }
        return PartialView("_AddEdit", dto);
    }


    [HttpPost]
    public async Task<IActionResult> Save(DoctorScheduleRequestDto dto)
    {
        try
        {
            var result = await _httpService.PostAsync<ApiResponseDto<string>>("api/doctorschedules/save", dto);
            return Json(new
            {
                success = result?.Success ?? false,
                message = result?.Message ?? "Save failed.",
                url = Url.Action("List", new { doctorId = dto.DoctorId })
            });
        }
        catch (HttpRequestException ex)
        {
            return Json(new { success = false, message = ExtractApiMessage(ex.Message), url = "" });
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
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _httpService.DeleteAsync<ApiResponseDto<string>>($"api/doctorschedules/{id}");
        return Json(new { success = result?.Success ?? false, message = result?.Message });
    }
}