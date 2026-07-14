// Api/Controllers/PatientsController.cs
using ClinicMS.Application.DTOs.Patient;
using ClinicMS.Application.Interfaces;
using ClinicMS.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _service;

        public PatientsController(IPatientService service)
        {
            _service = service;
        }

        // GET api/patients — any authenticated role can read (Doctor needs view access)
        [HttpGet]
        public async Task<IActionResult> GetPatients([FromQuery] PatientFilterDto filter)
        {
            var result = await _service.GetPatientsAsync(filter);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<PaginatedResult<PatientListItemDto>>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<PaginatedResult<PatientListItemDto>>.SuccessResponse(result.Data!));
        }

        // GET api/patients/{id} — any authenticated role can read
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatient(string id)
        {
            var result = await _service.GetPatientByIdAsync(id);

            if (!result.IsSuccess)
                return NotFound(ApiResponseDto<PatientResponseDto>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<PatientResponseDto>.SuccessResponse(result.Data!));
        }

        // POST api/patients — write, Admin/Receptionist only
        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> CreatePatient([FromBody] PatientRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<string>.ErrorResponse("Invalid data"));

            var result = await _service.CreatePatientAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return StatusCode(201, ApiResponseDto<string>.SuccessResponse(result.Data!, "Patient created successfully"));
        }

        // PUT api/patients/{id} — write, Admin/Receptionist only
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> UpdatePatient(string id, [FromBody] PatientRequestDto dto)
        {
            dto.Id = id;

            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<string>.ErrorResponse("Invalid data"));

            var result = await _service.UpdatePatientAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse("Patient updated successfully"));
        }

        // DELETE api/patients/{id} — soft delete (BR7), Admin/Receptionist only
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> DeactivatePatient(string id)
        {
            var result = await _service.DeactivatePatientAsync(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse("Patient deactivated successfully"));
        }

        // POST api/patients/{id}/reactivate — Admin/Receptionist only
        [HttpPost("{id}/reactivate")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> ReactivatePatient(string id)
        {
            var result = await _service.ReactivatePatientAsync(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse("Patient reactivated successfully"));
        }
    }
}