using ClinicMS.Application.DTOs.Patient;
using ClinicMS.Shared.Common;
using ClinicMS.Web.ApiClients;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Web.Controllers
{
    public class PatientsController : Controller
    {
        private readonly IHttpService _httpService;

        public PatientsController(IHttpService httpService)
        {
            _httpService = httpService;
        }

        // GET: /Patients
        public async Task<IActionResult> Index()
        {
            var filter = new PatientFilterDto();
            var result = await _httpService.GetAsync<ApiResponseDto<PaginatedResult<PatientListItemDto>>>(
                $"api/patients?PageNo={filter.PageNo}&PageSize={filter.PageSize}");

            return View(result?.Data ?? new PaginatedResult<PatientListItemDto>());
        }

        // GET: /Patients/List?SearchTerm=x&IsActive=true&PageNo=2
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] PatientFilterDto filter)
        {
            var query = $"api/patients?PageNo={filter.PageNo}&PageSize={filter.PageSize}";
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query += $"&SearchTerm={Uri.EscapeDataString(filter.SearchTerm)}";
            if (filter.IsActive.HasValue)
                query += $"&IsActive={filter.IsActive.Value}";

            var result = await _httpService.GetAsync<ApiResponseDto<PaginatedResult<PatientListItemDto>>>(query);
            return PartialView("_List", result?.Data ?? new PaginatedResult<PatientListItemDto>());
        }

        // GET: /Patients/AddEdit?id=
        [HttpGet]
        public async Task<IActionResult> AddEdit(string? id)
        {
            var dto = new PatientRequestDto();
            if (!string.IsNullOrEmpty(id))
            {
                var result = await _httpService.GetAsync<ApiResponseDto<PatientResponseDto>>($"api/patients/{id}");
                if (result?.Data != null)
                {
                    dto.Id = result.Data.Id;
                    dto.FirstName = result.Data.FirstName;
                    dto.LastName = result.Data.LastName;
                    dto.DateOfBirth = result.Data.DateOfBirth;
                    dto.Gender = result.Data.Gender;
                    dto.BloodGroup = result.Data.BloodGroup;
                    dto.Phone = result.Data.Phone;
                    dto.Email = result.Data.Email;
                    dto.Address = result.Data.Address;
                }
            }
            return PartialView("_AddEdit", dto);
        }

        // POST: /Patients/Save
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Save(PatientRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddEdit", dto);
            }

            try
            {
                ApiResponseDto<string>? result;
                if (string.IsNullOrEmpty(dto.Id))
                    result = await _httpService.PostAsync<ApiResponseDto<string>>("api/patients", dto);
                else
                    result = await _httpService.PutAsync<ApiResponseDto<string>>($"api/patients/{dto.Id}", dto);

                if (result == null || !result.Success)
                {
                    ModelState.AddModelError("", result?.Message ?? "Failed to save patient");
                    return PartialView("_AddEdit", dto);
                }

                return Json(new { success = true, url = Url.Action("List") });
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("Unauthorized"))
            {
                return StatusCode(401);   // let browser's real ajax error handler catch this — see Bug 3 fix below
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return PartialView("_AddEdit", dto);
            }
        }

        // POST: /Patients/Deactivate/{id}
        [HttpPost]
        public async Task<IActionResult> Deactivate(string id)
        {
            await _httpService.DeleteAsync<ApiResponseDto<string>>($"api/patients/{id}");

            var filter = new PatientFilterDto();
            var result = await _httpService.GetAsync<ApiResponseDto<PaginatedResult<PatientListItemDto>>>(
                $"api/patients?PageNo={filter.PageNo}&PageSize={filter.PageSize}");

            return PartialView("_List", result?.Data ?? new PaginatedResult<PatientListItemDto>());
        }

        [HttpPost]
        public async Task<IActionResult> Reactivate(string id)
        {
            await _httpService.PostAsync<ApiResponseDto<string>>($"api/patients/{id}/reactivate", new { });

            var filter = new PatientFilterDto();
            var result = await _httpService.GetAsync<ApiResponseDto<PaginatedResult<PatientListItemDto>>>(
                $"api/patients?PageNo={filter.PageNo}&PageSize={filter.PageSize}");

            return PartialView("_List", result?.Data ?? new PaginatedResult<PatientListItemDto>());
        }
    }
}