using Blazored.LocalStorage;
using LightVault.WebApplication.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace LightVault.WebApplication.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authProvider;
        public string? Token { get; private set; }
        public string? Username { get; private set; }
        public string? Role { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrWhiteSpace(Token);

        public AuthService(HttpClient http, ILocalStorageService localStorage, AuthenticationStateProvider authProvider)
        {
            _http = http;
            _localStorage = localStorage;
            _authProvider = authProvider;
        }

        public async Task InitializeAsync()
        {
            Token = await _localStorage.GetItemAsStringAsync("authToken");

            if (string.IsNullOrWhiteSpace(Token))
            {
                ClearAuth();
                return;
            }

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Token);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(Token);

            Username = jwt.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            Role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        }

        private void ClearAuth()
        {
            Token = null;
            Username = null;
            Role = null;
        }

        public async Task<bool> Login(string username, string password)
        {
            var resp = await _http.PostAsJsonAsync("/api/auth/login",
                new LoginRequest { Username = username, Password = password });

            if (!resp.IsSuccessStatusCode)
                return false;

            var data = await resp.Content.ReadFromJsonAsync<LoginResponse>();
            if (data is null)
                return false;

            Token = data.Token;
            Username = data.Username;
            Role = data.Role;

            await _localStorage.SetItemAsStringAsync("authToken", Token);

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Token);

            await ((ApiAuthenticationStateProvider)_authProvider).NotifyAuthChanged();

            return true;
        }

        public async Task Logout()
        {
            ClearAuth();

            await _localStorage.RemoveItemAsync("authToken");
            _http.DefaultRequestHeaders.Authorization = null;
            await ((ApiAuthenticationStateProvider)_authProvider).NotifyAuthChanged();
        }
        public bool IsUserAdmin()
        {
            return Role == "Admin";
        }

        public bool IsAuditorRole()
        {
            return Role == "Auditor";
        }
    }
}