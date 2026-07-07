using ClinicMS.Application.DTOs.Doctor;
using ClinicMS.Application.Interfaces;
using ClinicMS.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _service;

        public DoctorsController(IDoctorService service)
        {
            _service = service;
        }

        // GET api/doctors
        [HttpGet]
        public async Task<IActionResult> GetDoctors(
            [FromQuery] DoctorFilterDto filter)
        {
            var result = await _service.GetDoctorsAsync(filter);
            if (!result.IsSuccess)
                return BadRequest(
                    ApiResponseDto<PaginatedResult<DoctorListItemDto>>
                    .ErrorResponse(result.ErrorMessage));

            return Ok(
                ApiResponseDto<PaginatedResult<DoctorListItemDto>>
                .SuccessResponse(result.Data!));
        }

        // GET api/doctors/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctor(string id)
        {
            var result = await _service.GetDoctorByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(
                    ApiResponseDto<DoctorResponseDto>
                    .ErrorResponse(result.ErrorMessage));

            return Ok(
                ApiResponseDto<DoctorResponseDto>
                .SuccessResponse(result.Data!));
        }

        // GET api/doctors/bydepartment/{departmentId}
        [HttpGet("bydepartment/{departmentId}")]
        public async Task<IActionResult> GetByDepartment(
            string departmentId)
        {
            var result = await _service
                .GetDoctorsByDepartmentAsync(departmentId);

            if (!result.IsSuccess)
                return BadRequest(
                    ApiResponseDto<List<DoctorListItemDto>>
                    .ErrorResponse(result.ErrorMessage));

            // CustomAjax cascading dropdown expects:
            // [{Value: "id", Text: "display name"}]
            var dropdownItems = result.Data!.Select(d => new
            {
                Value = d.Id,
                Text = $"{d.FullName} ({d.Specialization})"
            });

            return Ok(dropdownItems);
        }

        // POST api/doctors
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDoctor(
            [FromBody] DoctorRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    ApiResponseDto<string>
                    .ErrorResponse("Invalid data"));

            var result = await _service.CreateDoctorAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(
                    ApiResponseDto<string>
                    .ErrorResponse(result.ErrorMessage));

            return StatusCode(201,
                ApiResponseDto<string>
                .SuccessResponse(result.Data!,
                    "Doctor created successfully"));
        }

        // PUT api/doctors/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDoctor(
            string id, [FromBody] DoctorRequestDto dto)
        {
            dto.Id = id;
            if (!ModelState.IsValid)
                return BadRequest(
                    ApiResponseDto<string>
                    .ErrorResponse("Invalid data"));

            var result = await _service.UpdateDoctorAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(
                    ApiResponseDto<string>
                    .ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>
                .SuccessResponse("Doctor updated successfully"));
        }

        // DELETE api/doctors/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateDoctor(string id)
        {
            var result = await _service.DeactivateDoctorAsync(id);
            if (!result.IsSuccess)
                return BadRequest(
                    ApiResponseDto<string>
                    .ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>
                .SuccessResponse("Doctor deactivated"));
        }

        // PUT api/doctors/{id}/reactivate
        [HttpPut("{id}/reactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReactivateDoctor(string id)
        {
            var result = await _service.ReactivateDoctorAsync(id);
            if (!result.IsSuccess)
                return BadRequest(
                    ApiResponseDto<string>
                    .ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>
                .SuccessResponse("Doctor reactivated"));
        }
    }
}