using ClinicMS.Application.DTOs.DoctorSchedule;
using ClinicMS.Application.Interfaces;
using ClinicMS.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorSchedulesController : ControllerBase
    {
        private readonly IDoctorScheduleService _service;
        public DoctorSchedulesController(IDoctorScheduleService service) => _service = service;

        [HttpGet("by-doctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctor(string doctorId)
        {
            var result = await _service.GetByDoctorAsync(doctorId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<List<DoctorScheduleListItemDto>>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<List<DoctorScheduleListItemDto>>.SuccessResponse(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponseDto<DoctorScheduleResponseDto>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<DoctorScheduleResponseDto>.SuccessResponse(result.Data!));
        }

        [HttpPost("save")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Save([FromBody] DoctorScheduleRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<string>.ErrorResponse("Invalid data"));

            var result = await _service.SaveAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse(result.Data!, "Schedule saved successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse("Schedule deleted successfully"));
        }
    }
}