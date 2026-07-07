using ClinicMS.Application.DTOs;
using ClinicMS.Application.Interfaces;
using ClinicMS.Shared.Common;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<LoginResponseDto>.ErrorResponse("Invalid request"));

            var result = await _authService.LoginAsync(dto);
            if (!result.IsSuccess)
                return Unauthorized(ApiResponseDto<LoginResponseDto>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<LoginResponseDto>.SuccessResponse(result.Data!));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
            if (!result.IsSuccess)
                return Unauthorized(ApiResponseDto<LoginResponseDto>.ErrorResponse(result.ErrorMessage));

            return Ok(ApiResponseDto<LoginResponseDto>.SuccessResponse(result.Data!));
        }
    }
}