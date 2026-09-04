using ClinicMS.Application.DTOs.DoctorSchedule;
using ClinicMS.Shared.Common;
using ClinicMS.Web.ApiClients;
using ClinicMS.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
public class DoctorSchedulesController : Controller
{
    private readonly IHttpService _httpService;
    public DoctorSchedulesController(IHttpService httpService) => _httpService = httpService;


    [HttpGet]
    [Authorize(Roles = "Doctor")]
    [HttpGet]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> MySchedule()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var myDoc = await _httpService.GetAsync<ApiResponseDto<string>>($"api/doctors/by-user/{userId}");

        if (string.IsNullOrEmpty(myDoc?.Data))
            return NotFound();

        return RedirectToAction("Index", new { doctorId = myDoc.Data });
    }
    public async Task<IActionResult> Index(string doctorId)
    {
        if (User.IsInRole("Doctor") && !User.IsInRole("Admin") && !User.IsInRole("Receptionist"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myDoc = await _httpService.GetAsync<ApiResponseDto<string>>($"api/doctors/by-user/{userId}");
            if (myDoc?.Data != doctorId)
                return Forbid();
        }

        var result = await _httpService.GetAsync<ApiResponseDto<List<DoctorScheduleListItemDto>>>(
            $"api/doctorschedules/by-doctor/{doctorId}");
        ViewBag.DoctorId = doctorId;
        ViewBag.CanEdit = User.IsInRole("Admin");
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
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Save(DoctorScheduleRequestDto dto)
    {
        try
        {
            var result = await _httpService.PostAsync<ApiResponseDto<string>>("api/doctorschedules/save", dto);

            if (result?.Success != true)
            {
                ModelState.AddModelError("", result?.Message ?? "Save failed.");
                return PartialView("_AddEdit", dto);
            }

            return Json(new { success = true, url = Url.Action("List", new { doctorId = dto.DoctorId }) });
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError("", ApiErrorHelper.ExtractApiMessage(ex.Message));
            return PartialView("_AddEdit", dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id, string doctorId)
    {
        await _httpService.DeleteAsync<ApiResponseDto<string>>($"api/doctorschedules/{id}");

        var result = await _httpService.GetAsync<ApiResponseDto<List<DoctorScheduleListItemDto>>>(
            $"api/doctorschedules/by-doctor/{doctorId}");
        ViewBag.DoctorId = doctorId;
        return PartialView("_List", result?.Data ?? new List<DoctorScheduleListItemDto>());
    }
}