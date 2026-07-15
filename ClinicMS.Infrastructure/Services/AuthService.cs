using ClinicMS.Application.DTOs;
using ClinicMS.Application.Interfaces;
using ClinicMS.Domain.Entities;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ClinicMS.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !user.IsActive)
                return Result<LoginResponseDto>.Fail("Invalid email or password");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Result<LoginResponseDto>.Fail("Invalid email or password");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            var token = GenerateJwtToken(user, role);
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

            var response = new LoginResponseDto
            {
                UserId = user.Id,
                Token = token,
                RefreshToken = refreshToken,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = role,
                Expiry = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpiryMinutes"))
            };

            return Result<LoginResponseDto>.Ok(response);
        }

        public async Task<Result<LoginResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
                return Result<LoginResponseDto>.Fail("Invalid or expired refresh token");

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            if (user == null || !user.IsActive)
                return Result<LoginResponseDto>.Fail("User not found or inactive");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            // Revoke old refresh token, issue new one (rotation - more secure)
            storedToken.IsRevoked = true;
            var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

            var newAccessToken = GenerateJwtToken(user, role);

            var response = new LoginResponseDto
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = role,
                Expiry = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpiryMinutes"))
            };

            return Result<LoginResponseDto>.Ok(response);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(string userId)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken.Token;
        }

        private string GenerateJwtToken(ApplicationUser user, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpiryMinutes")),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}