namespace LightVault.DemoApp.Services
{
    using System.Net.Http.Headers;
    using System.Net.Http.Json;

    public class LightVaultClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;
        private string? _jwt;

        public LightVaultClient(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _cfg = cfg;
        }

        public async Task<bool> EnsureJwtAsync()
        {
            if (!string.IsNullOrWhiteSpace(_jwt))
                return true;

            var apiKey = _cfg["LightVault:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return false;

            var resp = await _http.PostAsJsonAsync("/api/auth/service-login", new { apiKey });
            if (!resp.IsSuccessStatusCode)
                return false;

            var data = await resp.Content.ReadFromJsonAsync<ServiceLoginResponse>();
            _jwt = data?.Token;

            return !string.IsNullOrWhiteSpace(_jwt);
        }

        public async Task<string?> GetSecretAsync(string key)
        {
            var ok = await EnsureJwtAsync();
            if (!ok) return null;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwt);

            var secret = await _http.GetFromJsonAsync<SecretResponse>($"/api/secrets/by-name/{key}");
            return secret?.Value;
        }

        private class ServiceLoginResponse
        {
            public string Token { get; set; } = "";
            public int ExpiresInMinutes { get; set; }
        }

        private class SecretResponse
        {
            public string Value { get; set; } = "";
        }
    }
}
