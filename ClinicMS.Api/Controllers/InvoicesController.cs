using ClinicMS.Application.DTOs.Invoice;
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
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _service;
        public InvoicesController(IInvoiceService service) => _service = service;

        [HttpGet(Name = "GetInvoicesPaged")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetInvoices([FromQuery] InvoiceFilterDto filter)
        {
            var result = await _service.GetInvoicesAsync(filter);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<PaginatedResult<InvoiceListItemDto>>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<PaginatedResult<InvoiceListItemDto>>.SuccessResponse(result.Data!));
        }

        [HttpGet("{id}", Name = "GetInvoiceById")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponseDto<InvoiceResponseDto>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<InvoiceResponseDto>.SuccessResponse(result.Data!));
        }

        [HttpGet("by-appointment/{appointmentId}", Name = "GetInvoiceByAppointment")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetByAppointment(string appointmentId)
        {
            var result = await _service.GetByAppointmentIdAsync(appointmentId);
            if (!result.IsSuccess)
                return NotFound(ApiResponseDto<InvoiceResponseDto>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<InvoiceResponseDto>.SuccessResponse(result.Data!));
        }


        [HttpPost(Name = "GenerateInvoice")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Generate([FromBody] InvoiceRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<string>.ErrorResponse("Invalid data"));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.GenerateAsync(dto, userId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return StatusCode(201, ApiResponseDto<string>.SuccessResponse(result.Data!, "Invoice generated successfully"));
        }
        [HttpPost("{id}/pay", Name = "PayInvoice")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Pay(string id, [FromBody] InvoicePayDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<string>.ErrorResponse("Invalid data"));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.PayAsync(id, dto, userId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse("Invoice paid successfully"));
        }
    }
}