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
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST /Auth/Login — processes login form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            // Call Api login endpoint via HttpService
            var result = await _httpService
                .PostAsync<ApiResponseDto<LoginResponseDto>>(
                    "/api/auth/login", dto);

            // Api unreachable or returned error
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
                new Claim(ClaimTypes.NameIdentifier, loginData.Email),
                new Claim(ClaimTypes.Name, loginData.FullName),
                new Claim(ClaimTypes.Email, loginData.Email),
                new Claim(ClaimTypes.Role, loginData.Role),
                new Claim("JwtToken", loginData.Token)
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in = write encrypted cookie to browser
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = loginData.Expiry
                });

            return RedirectToAction("Index", "Home");
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