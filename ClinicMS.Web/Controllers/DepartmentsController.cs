using ClinicMS.Application.DTOs.Department;
using ClinicMS.Shared.Common;
using ClinicMS.Web.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Web.Controllers
{
    [Authorize]
    // All department pages need login
    public class DepartmentsController : Controller
    {
        private readonly IHttpService _httpService;

        public DepartmentsController(IHttpService httpService)
        {
            _httpService = httpService;
        }

        // GET /Departments
        // Returns full page (Index.cshtml)
        public IActionResult Index()
        {
            return View();
        }

        // GET /Departments/List
        public async Task<IActionResult> List(DepartmentFilterDto filter)
        {
            var queryParams = new List<string>
    {
        $"PageNo={filter.PageNo}",
        $"PageSize={filter.PageSize}"
    };

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                queryParams.Add(
                    $"SearchTerm={Uri.EscapeDataString(filter.SearchTerm)}");

            if (filter.IsActive.HasValue)
                queryParams.Add(
                    $"IsActive={filter.IsActive.Value.ToString().ToLower()}");
            // .ToLower() = sends "true"/"false" not "True"/"False"
            // API model binder handles lowercase reliably

            var queryString = string.Join("&", queryParams);

            var result = await _httpService
                .GetAsync<ApiResponseDto<PaginatedResult<DepartmentListItemDto>>>(
                    $"/api/departments?{queryString}");

            var data = result?.Data
                ?? new PaginatedResult<DepartmentListItemDto>();

            return PartialView("_List", data);
        }
        // GET /Departments/Add
        // Called when clicking [+ Add New] button
        [Authorize(Roles = "Admin")]
        public IActionResult Add()
        {
            // Empty DTO = blank form
            return PartialView("_AddEdit",
                new DepartmentRequestDto());
        }

        // POST /Departments/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(DepartmentRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                // Return partial WITH validation errors
                // CustomAjax.js bindForm() keeps modal open
                return PartialView("_AddEdit", dto);
            }

            var result = await _httpService
                .PostAsync<ApiResponseDto<string>>(
                    "/api/departments", dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Failed to create department");
                return PartialView("_AddEdit", dto);
            }

            TempData["MsgCode"] = "success";
            TempData["Msg"] = "Department created successfully";

            return Json(new
            {
                success = true,
                url = Url.Action("List", "Departments")
            });
        }

        // GET /Departments/Edit/{id}
        // Called when clicking Edit button on grid row
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var result = await _httpService
                .GetAsync<ApiResponseDto<DepartmentResponseDto>>(
                    $"/api/departments/{id}");

            if (result == null || !result.Success)
                return PartialView("_AddEdit",
                    new DepartmentRequestDto());

            // Map ResponseDto → RequestDto for edit form
            var dto = new DepartmentRequestDto
            {
                Id = result.Data!.Id,
                Name = result.Data.Name,
                Description = result.Data.Description,
                IsActive = result.Data.IsActive
            };

            return PartialView("_AddEdit", dto);
        }

        // POST /Departments/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            string id, DepartmentRequestDto dto)
        {
            dto.Id = id;

            if (!ModelState.IsValid)
                return PartialView("_AddEdit", dto);

            var result = await _httpService
                .PutAsync<ApiResponseDto<string>>(
                    $"/api/departments/{id}", dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Failed to update department");
                return PartialView("_AddEdit", dto);
            }

            TempData["MsgCode"] = "success";
            TempData["Msg"] = "Department updated successfully";

            return Json(new
            {
                success = true,
                url = Url.Action("List", "Departments")
            });
        }

        // POST /Departments/Deactivate/{id}
        // Called by CustomAjax delete confirm modal
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(string id)
        {
            var result = await _httpService
                .DeleteAsync<ApiResponseDto<string>>(
                    $"/api/departments/{id}");

            // After deactivate → return updated grid partial
            // CustomAjax replaces grid with this
            TempData["MsgCode"] = result?.Success == true
                ? "success" : "error";
            TempData["Msg"] = result?.Message
                ?? "Operation failed";

            return await List(new DepartmentFilterDto());
        }
        // Returns JSON list for dropdown — called by doctors.js
        [HttpGet]
        public async Task<IActionResult> ActiveList()
        {
            var result = await _httpService
                .GetAsync<ApiResponseDto<List<DepartmentListItemDto>>>(
                    "/api/departments/active");

            return Json(result?.Data ?? new List<DepartmentListItemDto>());
        }
    }
}