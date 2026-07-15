using System.Net.Http.Headers;
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
        private void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.User
                .FindFirst("JwtToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            AttachToken();
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
            AttachToken();
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
            AttachToken();
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
            AttachToken();
            var response = await _httpClient.DeleteAsync(endpoint);
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
        }
    }
}