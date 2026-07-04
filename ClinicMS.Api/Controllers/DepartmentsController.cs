using ClinicMS.Application.DTOs.Department;
using ClinicMS.Application.Interfaces;
using ClinicMS.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _service;

        public DepartmentsController(IDepartmentService service)
        {
            _service = service;
        }

        // GET api/departments?SearchTerm=cardio&PageNo=1&PageSize=10
        [HttpGet]
        public async Task<IActionResult> GetDepartments(
            [FromQuery] DepartmentFilterDto filter)
        {
            var result = await _service.GetDepartmentsAsync(filter);

            if (!result.IsSuccess)
                return BadRequest(
                    ApiResponseDto<PaginatedResult<DepartmentListItemDto>>
                    .ErrorResponse(result.ErrorMessage));

            return Ok(
                ApiResponseDto<PaginatedResult<DepartmentListItemDto>>
                .SuccessResponse(result.Data!));
        }

        // GET api/departments/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveDepartments()
        {
            var result = await _service.GetActiveDepartmentsAsync();

            if (!result.IsSuccess)
                return BadRequest(
                    ApiResponseDto<List<DepartmentListItemDto>>
                    .ErrorResponse(result.ErrorMessage));

            return Ok(
                ApiResponseDto<List<DepartmentListItemDto>>
                .SuccessResponse(result.Data!));
        }

        // GET api/departments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartment(string id)
        {
            var result = await _service.GetDepartmentByIdAsync(id);

            if (!result.IsSuccess)
                return NotFound(
                    ApiResponseDto<DepartmentResponseDto>
                    .ErrorResponse(result.ErrorMessage));

            return Ok(
                ApiResponseDto<DepartmentResponseDto>
                .SuccessResponse(result.Data!));
        }

        // POST api/departments
        // Creates new department
        // [Authorize(Roles="Admin")] = ONLY Admin can create
        // Receptionist/Doctor = 403 Forbidden
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDepartment(
            [FromBody] DepartmentRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    ApiResponseDto<string>
                    .ErrorResponse("Invalid data"));

            var result = await _service.CreateDepartmentAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(
                    ApiResponseDto<string>
                    .ErrorResponse(result.ErrorMessage));

            // 201 Created = correct HTTP status for new resource
            return StatusCode(201,
                ApiResponseDto<string>
                .SuccessResponse(result.Data!,
                    "Department created successfully"));
        }

        // PUT api/departments/{id}
        // Updates existing department
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDepartment(
            string id, [FromBody] DepartmentRequestDto dto)
        {
            // Sync URL id with dto.Id for safety
            // Prevents mismatch between route id and body id
            dto.Id = id;

            if (!ModelState.IsValid)
                return BadRequest(
                    ApiResponseDto<string>
                    .ErrorResponse("Invalid data"));

            var result = await _service.UpdateDepartmentAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(
                    ApiResponseDto<string>
                    .ErrorResponse(result.ErrorMessage));

            return Ok(
                ApiResponseDto<string>
                .SuccessResponse("Department updated successfully"));
        }

        // DELETE api/departments/{id}
        // Deactivates (soft disable) — never hard deletes
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateDepartment(string id)
        {
            var result = await _service
                .DeactivateDepartmentAsync(id);

            if (!result.IsSuccess)
                return BadRequest(
                    ApiResponseDto<string>
                    .ErrorResponse(result.ErrorMessage));

            return Ok(
                ApiResponseDto<string>
                .SuccessResponse("Department deactivated successfully"));
        }
    }
}