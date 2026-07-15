using ClinicMS.Application.DTOs.Prescription;
using ClinicMS.Application.Interfaces;
using ClinicMS.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClinicMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionService _service;
        public PrescriptionsController(IPrescriptionService service) => _service = service;

        [HttpGet("by-appointment/{appointmentId}")]
        public async Task<IActionResult> GetByAppointment(string appointmentId)
        {
            var result = await _service.GetByAppointmentIdAsync(appointmentId);
            if (!result.IsSuccess)
                return NotFound(ApiResponseDto<PrescriptionResponseDto>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<PrescriptionResponseDto>.SuccessResponse(result.Data!));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create([FromBody] PrescriptionRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<string>.ErrorResponse("Invalid data"));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isAdmin = User.IsInRole("Admin");

            var result = await _service.CreateAsync(dto, userId, isAdmin);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return StatusCode(201, ApiResponseDto<string>.SuccessResponse(result.Data!, "Prescription saved"));
        }
    }
}