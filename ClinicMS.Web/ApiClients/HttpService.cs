using ClinicMS.Application.DTOs;
using ClinicMS.Shared.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ClinicMS.Web.ApiClients
{
    // HttpService = actual implementation of IHttpService.
    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // JSON options — camelCase matches what Api returns
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
            // This means: "userId" and "UserId" both deserialize correctly
        };

        public HttpService(HttpClient httpClient,
                          IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        // Attaches JWT token to request header.
        private async Task AttachToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) { return; }

            var expiryClaim = httpContext.User.FindFirst("TokenExpiry")?.Value;
            string? token;
            bool isTokenExpired = false;
            if (!string.IsNullOrEmpty(expiryClaim) &&
        DateTime.TryParse(expiryClaim, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiryTime))
            {
                isTokenExpired = DateTime.UtcNow >= expiryTime.ToUniversalTime().AddSeconds(-30);
            }
            if (isTokenExpired)
            {
                token = await TryRefreshTokenAsync();
            }
            else
            {
                token = httpContext.User
                    .FindFirst("JwtToken")?.Value;
            }


            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            await AttachToken();
            var response = await _httpClient.GetAsync(endpoint);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Api call failed: {response.StatusCode} — {json}");
            }

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            await AttachToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Api rejected it — surface real reason instead of silent empty deserialize
                throw new HttpRequestException($"Api call failed: {response.StatusCode} — {responseJson}");
            }

            return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            await AttachToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoint, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Api call failed: {response.StatusCode} — {responseJson}");
            }

            return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
        }

        public async Task<T?> DeleteAsync<T>(string endpoint)
        {
            await AttachToken();
            var response = await _httpClient.DeleteAsync(endpoint);
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
        }


        private async Task<string?> TryRefreshTokenAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var refreshToken = httpContext?.User.FindFirst("refresh_token")?.Value;
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                await httpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return null;
            }
            var payload = new RefreshRequestDto
            {
                RefreshToken = refreshToken
            };
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync("/api/auth/refresh", content);
            if (!response.IsSuccessStatusCode)
            {
                await httpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return null;
            }
            var text = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponseDto<LoginResponseDto>>(text, _jsonOptions);
            if (result == null || !result.Success || result.Data == null)
            {
                await httpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return null;
            }
            await UpdateCookieAsync(httpContext!, result.Data);
            return result.Data.Token;
        }
        public static async Task UpdateCookieAsync(HttpContext httpContext, LoginResponseDto data)
        {
            var oldPrincipal = httpContext.User;
            var claims = oldPrincipal.Claims.Where(
                c => c.Type != "JwtToken"
                && c.Type != "TokenExpiry"
                && c.Type != "refresh_token"
                && c.Type != "refresh_token_expiration"
                ).ToList();
            claims.Add(new Claim("JwtToken", data.Token));
            claims.Add(new Claim("TokenExpiry", data.Expiry.ToString("o")));
            if (!string.IsNullOrEmpty(data.RefreshToken))
            {
                claims.Add(new Claim("refresh_token", data.RefreshToken));
            }
            if (data.RefreshTokenExpiration.HasValue)
            {
                claims.Add(new Claim("refresh_token_expiration", data.RefreshTokenExpiration.Value.ToString("o")));
            }
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var newPrincipal = new ClaimsPrincipal(identity);
            httpContext.User = newPrincipal;
            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newPrincipal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = data.RefreshTokenExpiration
            });
        }

    }
}

