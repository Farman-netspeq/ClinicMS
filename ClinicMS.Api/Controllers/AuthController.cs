using ClinicMS.Application.DTOs;
using ClinicMS.Domain.Entities;
using ClinicMS.Shared.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClinicMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<LoginResponseDto>
                    .ErrorResponse("Invalid request"));

            // find user by email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !user.IsActive)
                return Unauthorized(ApiResponseDto<LoginResponseDto>
                    .ErrorResponse("Invalid email or password"));

            // check password
            var result = await _signInManager.CheckPasswordSignInAsync(
                user, dto.Password, lockoutOnFailure: false);

            if (!result.Succeeded)
                return Unauthorized(ApiResponseDto<LoginResponseDto>
                    .ErrorResponse("Invalid email or password"));

            // get user's role
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            //  generate JWT token
            var token = GenerateJwtToken(user, role);

            var response = new LoginResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = role,
                Expiry = DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>("Jwt:ExpiryMinutes"))
            };

            return Ok(ApiResponseDto<LoginResponseDto>.SuccessResponse(response));
        }

        // Builds JWT token with user info as "claims"
        private string GenerateJwtToken(ApplicationUser user, string role)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>("Jwt:ExpiryMinutes")),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}