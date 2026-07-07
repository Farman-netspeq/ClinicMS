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

        // GET api/patients?SearchTerm=john&IsActive=true&PageNo=1&PageSize=10
        [HttpGet]
        public async Task<IActionResult> GetPatients([FromQuery] PatientFilterDto filter)
        {
            var result = await _service.GetPatientsAsync(filter);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<PaginatedResult<PatientListItemDto>>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<PaginatedResult<PatientListItemDto>>.SuccessResponse(result.Data!));
        }

        // GET api/patients/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatient(string id)
        {
            var result = await _service.GetPatientByIdAsync(id);

            if (!result.IsSuccess)
                return NotFound(ApiResponseDto<PatientResponseDto>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<PatientResponseDto>.SuccessResponse(result.Data!));
        }

        // POST api/patients
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] PatientRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<string>.ErrorResponse("Invalid data"));

            var result = await _service.CreatePatientAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return StatusCode(201, ApiResponseDto<string>.SuccessResponse(result.Data!, "Patient created successfully"));
        }

        // PUT api/patients/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(string id, [FromBody] PatientRequestDto dto)
        {
            dto.Id = id;   // sync route id with body, prevents mismatch/tampering

            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<string>.ErrorResponse("Invalid data"));

            var result = await _service.UpdatePatientAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse("Patient updated successfully"));
        }

        // DELETE api/patients/{id}  — soft delete (BR7)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivatePatient(string id)
        {
            var result = await _service.DeactivatePatientAsync(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse("Patient deactivated successfully"));
        }

        // POST api/patients/{id}/reactivate
        [HttpPost("{id}/reactivate")]
        public async Task<IActionResult> ReactivatePatient(string id)
        {
            var result = await _service.ReactivatePatientAsync(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<string>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<string>.SuccessResponse("Patient reactivated successfully"));
        }
    }
}