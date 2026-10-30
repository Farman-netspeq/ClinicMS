using ClinicMS.Application.DTOs.Department;
using ClinicMS.Application.DTOs.Doctor;
using ClinicMS.Shared.Common;
using ClinicMS.Web.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        // Web DoctorsController
        public async Task<IActionResult> Index()
        {
            var result = await _httpService.GetAsync<ApiResponseDto<PaginatedResult<DoctorListItemDto>>>(
                "/api/doctors?PageNo=1&PageSize=10");
            var depts = await _httpService.GetAsync<ApiResponseDto<List<DepartmentListItemDto>>>("/api/departments/active");
            //ViewBag.Departments = depts?.Data ?? new List<DepartmentListItemDto>();
            //ViewBag.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dto = new DoctorListPageDto
            {
                Doctors = result?.Data??new PaginatedResult<DoctorListItemDto>(),
                Departments = depts?.Data??new List<DepartmentListItemDto>()
            };
            return View(dto);
        }

        public async Task<IActionResult> List(DoctorFilterDto filter)
        {
            var queryParams = new List<string> { $"PageNo={filter.PageNo}", $"PageSize={filter.PageSize}" };

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                queryParams.Add($"SearchTerm={Uri.EscapeDataString(filter.SearchTerm)}");
            if (!string.IsNullOrWhiteSpace(filter.DepartmentId))
                queryParams.Add($"DepartmentId={filter.DepartmentId}");
            if (filter.IsActive.HasValue)
                queryParams.Add($"IsActive={filter.IsActive.Value.ToString().ToLower()}");

            var result = await _httpService
                .GetAsync<ApiResponseDto<PaginatedResult<DoctorListItemDto>>>(
                    $"/api/doctors?{string.Join("&", queryParams)}");


            var depts = await _httpService
                .GetAsync<ApiResponseDto<List<DepartmentListItemDto>>>("/api/departments/active");

            var pagedData = new DoctorListPageDto
            {
                Doctors = result?.Data ?? new PaginatedResult<DoctorListItemDto>(),
                Departments = depts?.Data ?? new List<DepartmentListItemDto>()
            };

            ViewBag.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);  

            return PartialView("_pvList", pagedData);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add()
        {
            var dto=new DoctorRequestDto();
            dto.Departments = await LoadDepartmentsList();
            return PartialView("_pvAddEdit", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(DoctorRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
               dto.Departments = await LoadDepartmentsList();
                return PartialView("_pvAddEdit", dto);
            }

            var result = await _httpService
                .PostAsync<ApiResponseDto<string>>(
                    "/api/doctors", dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Failed to create doctor");
                return Json(new
                {
                    success = false,
                    message = result?.Message ?? "Failed to create doctor"
                });
            }


            return Json(new
            {
                success = true,
                messageCode="success",
                message="Doctor Created Successfully",
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
               var emptyDTO = new DoctorRequestDto();
                emptyDTO.Departments = await LoadDepartmentsList();
                return PartialView("_pvAddEdit", emptyDTO);
            }

            var dto = new DoctorRequestDto
            {
                Id = result.Data!.Id,
                FullName = result.Data.FullName,
                DepartmentId = result.Data.DepartmentId,
                Specialization = result.Data.Specialization,
                LicenseNumber = result.Data.LicenseNumber,
                ConsultationFee = result.Data.ConsultationFee,
                IsActive = result.Data.IsActive,
                Departments = await LoadDepartmentsList()
                // Email/Password NOT prefilled — not editable
            };

            return PartialView("_pvAddEdit", dto);
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
                dto.Departments = await LoadDepartmentsList();
                return PartialView("_pvAddEdit", dto);
            }

            var result = await _httpService
                .PutAsync<ApiResponseDto<string>>(
                    $"/api/doctors/{id}", dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Failed to update doctor");
                dto.Departments = await LoadDepartmentsList();
                return PartialView("_pvAddEdit", dto);
            }

            return Json(new
            {
                success = true,
                messageCode = "success",
                message = "Doctor updated successfully",
                url = Url.Action("List", "Doctors")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
        private async Task<List<DepartmentListItemDto>> LoadDepartmentsList()
        {
            var depts = await _httpService
                .GetAsync<ApiResponseDto<List<DepartmentListItemDto>>>(
                    "/api/departments/active");

           var result = depts?.Data
                ?? new List<DepartmentListItemDto>();
            return result;
        }
    }
}