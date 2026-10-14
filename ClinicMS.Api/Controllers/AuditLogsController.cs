using ClinicMS.Application.DTOs.Audit;
using ClinicMS.Application.Interfaces;
using ClinicMS.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditService _service;
        public AuditLogsController(IAuditService service) => _service = service;

        [HttpPost("list", Name = "GetAuditLogsPaged")]
        public async Task<IActionResult> GetList([FromBody] AuditLogFilterDto filter)
        {
            var result = await _service.GetAuditLogsAsync(filter);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<PaginatedResult<AuditLogListItemDto>>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<PaginatedResult<AuditLogListItemDto>>.SuccessResponse(result.Data!));
        }
    }
}