using ClinicMS.Application.DTOs.Appointment;
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
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;
        public AppointmentsController(IAppointmentService service) => _service = service;

        // grid list — POST bc filter is complex obj, avoids long querystring
        [HttpPost("list", Name = "GetAppointmentsPaged")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetList([FromBody] AppointmentFilterDto filter)
        {
            var result = await _service.GetAppointmentsAsync(filter);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<PaginatedResult<AppointmentListItemDto>>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<PaginatedResult<AppointmentListItemDto>>.SuccessResponse(result.Data!));
        }

        [HttpGet("{id}", Name = "GetAppointmentById")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetAppointmentByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponseDto<AppointmentResponseDto>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<AppointmentResponseDto>.SuccessResponse(result.Data!));
        }

        // cascading dropdown step 3: doctor+date picked → slots
        [HttpGet("slots", Name = "GetAvailableSlots")]
        public async Task<IActionResult> GetSlots([FromQuery] string doctorId, [FromQuery] DateTime date)
        {
            var result = await _service.GetAvailableSlotsAsync(doctorId, date);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<List<AppointmentSlotDto>>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<List<AppointmentSlotDto>>.SuccessResponse(result.Data!));
        }

        [HttpPost("book", Name = "BookAppointment")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Book([FromBody] AppointmentRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m));
                var message = string.Join(" ", errors);
                return BadRequest(ApiResponseDto<string>.ErrorResponse(
                    string.IsNullOrWhiteSpace(message) ? "Invalid data" : message));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.CreateAppointmentAsync(dto, userId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse(result.Data!, "Appointment booked successfully"));
        }

        [HttpPost("{id}/cancel", Name = "CancelAppointment")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Cancel(string id, [FromBody] CancelAppointmentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.CancelAppointmentAsync(id, dto.CancelReason, userId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse("Appointment cancelled"));
        }
        // ── CHECK-IN ────────────────────────────────────────────
        [HttpPost("{id}/checkin")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> CheckIn(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.CheckInAppointmentAsync(id, userId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse("Appointment checked in"));
        }

        // ── COMPLETE ────────────────────────────────────────────
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Complete(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");

            var result = await _service.CompleteAppointmentAsync(id, userId, isAdmin);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse("Appointment completed"));
        }

        // ── NO-SHOW ─────────────────────────────────────────────
        [HttpPost("{id}/noshow")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> NoShow(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.MarkNoShowAsync(id, userId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse("Appointment marked as no-show"));
        }

        // ── DOCTOR DASHBOARD ─────────────────────────────────────
        [HttpGet("my-appointments")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetMyAppointments([FromQuery] DateTime? date)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.GetDoctorAppointmentsAsync(userId, date);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<List<AppointmentListItemDto>>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<List<AppointmentListItemDto>>.SuccessResponse(result.Data!));
        }
    }
}