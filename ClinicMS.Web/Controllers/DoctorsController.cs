using ClinicMS.Application.DTOs.Department;
using ClinicMS.Application.DTOs.Doctor;
using ClinicMS.Shared.Common;
using ClinicMS.Web.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Web.Controllers
{
    [Authorize]
    public class DoctorsController : Controller
    {
        private readonly IHttpService _httpService;

        public DoctorsController(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> List(DoctorFilterDto filter)
        {
            var queryParams = new List<string>
            {
                $"PageNo={filter.PageNo}",
                $"PageSize={filter.PageSize}"
            };

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                queryParams.Add(
                    $"SearchTerm={Uri.EscapeDataString(filter.SearchTerm)}");

            if (!string.IsNullOrWhiteSpace(filter.DepartmentId))
                queryParams.Add($"DepartmentId={filter.DepartmentId}");

            if (filter.IsActive.HasValue)
                queryParams.Add(
                    $"IsActive={filter.IsActive.Value.ToString().ToLower()}");

            var result = await _httpService
                .GetAsync<ApiResponseDto<PaginatedResult<DoctorListItemDto>>>(
                    $"/api/doctors?{string.Join("&", queryParams)}");

            var data = result?.Data
                ?? new PaginatedResult<DoctorListItemDto>();

            // Get departments for filter dropdown
            var depts = await _httpService
                .GetAsync<ApiResponseDto<List<DepartmentListItemDto>>>(
                    "/api/departments/active");

            ViewBag.Departments = depts?.Data
                ?? new List<DepartmentListItemDto>();

            return PartialView("_List", data);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add()
        {
            await LoadDepartmentsToViewBag();
            return PartialView("_AddEdit", new DoctorRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(DoctorRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadDepartmentsToViewBag();
                return PartialView("_AddEdit", dto);
            }

            var result = await _httpService
                .PostAsync<ApiResponseDto<string>>(
                    "/api/doctors", dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Failed to create doctor");
                await LoadDepartmentsToViewBag();
                return PartialView("_AddEdit", dto);
            }

            TempData["MsgCode"] = "success";
            TempData["Msg"] = "Doctor created successfully";

            return Json(new
            {
                success = true,
                url = Url.Action("List", "Doctors")
            });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var result = await _httpService
                .GetAsync<ApiResponseDto<DoctorResponseDto>>(
                    $"/api/doctors/{id}");

            if (result == null || !result.Success)
            {
                await LoadDepartmentsToViewBag();
                return PartialView("_AddEdit", new DoctorRequestDto());
            }

            var dto = new DoctorRequestDto
            {
                Id = result.Data!.Id,
                FullName = result.Data.FullName,
                DepartmentId = result.Data.DepartmentId,
                Specialization = result.Data.Specialization,
                LicenseNumber = result.Data.LicenseNumber,
                ConsultationFee = result.Data.ConsultationFee,
                IsActive = result.Data.IsActive
                // Email/Password NOT prefilled — not editable
            };

            await LoadDepartmentsToViewBag();
            return PartialView("_AddEdit", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            string id, DoctorRequestDto dto)
        {
            dto.Id = id;

            ModelState.Remove("Email");
            ModelState.Remove("Password");

            if (!ModelState.IsValid)
            {
                await LoadDepartmentsToViewBag();
                return PartialView("_AddEdit", dto);
            }

            var result = await _httpService
                .PutAsync<ApiResponseDto<string>>(
                    $"/api/doctors/{id}", dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Failed to update doctor");
                await LoadDepartmentsToViewBag();
                return PartialView("_AddEdit", dto);
            }

            TempData["MsgCode"] = "success";
            TempData["Msg"] = "Doctor updated successfully";

            return Json(new
            {
                success = true,
                url = Url.Action("List", "Doctors")
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(string id)
        {
            var result = await _httpService
                .DeleteAsync<ApiResponseDto<string>>(
                    $"/api/doctors/{id}");

            TempData["MsgCode"] = result?.Success == true
                ? "success" : "error";
            TempData["Msg"] = result?.Message ?? "Operation failed";

            return await List(new DoctorFilterDto());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reactivate(string id)
        {
            var result = await _httpService
                .PutAsync<ApiResponseDto<string>>(
                    $"/api/doctors/{id}/reactivate", new { });

            TempData["MsgCode"] = result?.Success == true
                ? "success" : "error";
            TempData["Msg"] = result?.Message ?? "Operation failed";

            return await List(new DoctorFilterDto());
        }

        // Private helper — loads departments into ViewBag
        // Used by Add/Edit actions for department dropdown in form
        private async Task LoadDepartmentsToViewBag()
        {
            var depts = await _httpService
                .GetAsync<ApiResponseDto<List<DepartmentListItemDto>>>(
                    "/api/departments/active");

            ViewBag.Departments = depts?.Data
                ?? new List<DepartmentListItemDto>();
        }
    }
}