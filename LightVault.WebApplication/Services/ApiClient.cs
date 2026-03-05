using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace LightVault.WebApplication.Services
{

    public class ApiClient
    {
        private readonly IServiceProvider _provider;
        private readonly ILocalStorageService _localStorage;

        private HttpClient Http => _provider.GetRequiredService<HttpClient>();

        public ApiClient(IServiceProvider provider, ILocalStorageService localStorage)
        {
            _provider = provider;
            _localStorage = localStorage;
        }

        /// <summary>
        /// Ensures Authorization header is attached for every request.
        /// </summary>
        private async Task<bool> AttachJwt()
        {
            var token = await GetJwt();

            if (!string.IsNullOrWhiteSpace(token))
            {
                Http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                return true;
            }

            Http.DefaultRequestHeaders.Authorization = null;
            return false;
        }

        private async Task<string> GetJwt()
        {
            try
            {
                var token = await _localStorage.GetItemAsStringAsync("authToken");
                return token;
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            await AttachJwt();
            var response = await Http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T?> PostAsync<T>(string url, object data)
        {
            await AttachJwt();
            var response = await Http.PostAsJsonAsync(url, data);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T?> PutAsync<T>(string url, object data)
        {
            await AttachJwt();
            var response = await Http.PutAsJsonAsync(url, data);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task PutAsync(string url, object data)
        {
            await AttachJwt();
            var response = await Http.PutAsJsonAsync(url, data);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(string url)
        {
            await AttachJwt();
            var response = await Http.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }
    }

}