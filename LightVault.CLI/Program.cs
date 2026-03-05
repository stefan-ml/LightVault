using System.Net.Http.Headers;
using System.Net.Http.Json;

var baseUrl = Environment.GetEnvironmentVariable("LIGHTVAULT_API_URL");
var username = Environment.GetEnvironmentVariable("LIGHTVAULT_USERNAME");
var password = Environment.GetEnvironmentVariable("LIGHTVAULT_PASSWORD");
var secretName = "Database";


if (string.IsNullOrWhiteSpace(baseUrl))
{
    Console.WriteLine("Environment variable LIGHTVAULT_API_URL is not set.");
    return;
}
if (string.IsNullOrWhiteSpace(username))
{
    Console.WriteLine("Environment variable LIGHTVAULT_USERNAME is not set.");
    return;
}
if (string.IsNullOrWhiteSpace(password))
{
    Console.WriteLine("Environment variable LIGHTVAULT_PASSWORD is not set.");
    return;
}

using var http = new HttpClient
{
    BaseAddress = new Uri(baseUrl)
};

Console.WriteLine("Authenticating to LightVault...");
Task.Delay(2500).Wait();

var loginResponse = await http.PostAsJsonAsync(
    "/api/auth/login",
    new LoginRequest(username, password)
);

if (!loginResponse.IsSuccessStatusCode)
{
    Console.WriteLine("Login failed");
    return;
}

var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", loginData!.Token);

Console.WriteLine($"Logged in as {loginData.Username} ({loginData.Role})");
Console.WriteLine($"Fetching secret '{secretName}' from Vault...");

var secretResponse =
    await http.GetAsync($"/api/secrets/by-name/{secretName}");

if (!secretResponse.IsSuccessStatusCode)
{
    Console.WriteLine("Secret not found or access denied");
    return;
}

var secret = await secretResponse.Content.ReadFromJsonAsync<SecretDto>();

Console.WriteLine("Secret retrieved successfully");

UseSecret(secret!.Value);

Console.WriteLine("CLI execution finished");


void UseSecret(string secretValue)
{
    Console.WriteLine(
        $"Secret: {secretValue}"
    );
}

record LoginRequest(string Username, string Password);

record LoginResponse(
    string Token,
    string Username,
    string Role
);

record SecretDto(
    Guid Id,
    string Name,
    string Value,
    int CurrentVersion
);
