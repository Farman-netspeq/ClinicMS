using ClinicMS.Application.DTOs;
using ClinicMS.Shared.Common;
using ClinicMS.Web.ApiClients;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClinicMS.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpService _httpService;

        public AuthController(IHttpService httpService)
        {
            _httpService = httpService;
        }

        // GET /Auth/Login — shows login form
        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in → go to dashboard
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // POST /Auth/Login — processes login form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            ApiResponseDto<LoginResponseDto>? result;
            try
            {
                result = await _httpService
                    .PostAsync<ApiResponseDto<LoginResponseDto>>(
                        "/api/auth/login", dto);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(dto);
            }

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Login failed. Please try again.");
                return View(dto);
            }

            var loginData = result.Data!;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginData.UserId),
                new Claim(ClaimTypes.Email, loginData.Email),
                new Claim(ClaimTypes.Name, loginData.FullName),
                new Claim(ClaimTypes.Role, loginData.Role),
                new Claim("JwtToken", loginData.Token),
                new Claim("TokenExpiry", loginData.Expiry.ToString("o"))
            };

            if (!string.IsNullOrEmpty(result.Data.RefreshToken))
            {
                claims.Add(new Claim("refresh_token", result.Data.RefreshToken));
            }
            if (result.Data.RefreshTokenExpiration.HasValue)
            {
                claims.Add(new Claim("refresh_token_expiration", result.Data.RefreshTokenExpiration.Value.ToString("o")));
            }
            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = loginData.RefreshTokenExpiration
                });

            return RedirectToAction("Index", "Dashboard");
        }
        // GET /Auth/Logout
        public async Task<IActionResult> Logout()
        {
            // Delete cookie from browser
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET /Auth/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}