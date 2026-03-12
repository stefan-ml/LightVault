using LightVault.API.DTOs;
using LightVault.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = LightVault.API.DTOs.LoginRequest;

namespace LightVault.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserAuthService _userAuthService;
    private readonly IServiceAccountAuthService _serviceAccountAuthService;

    public AuthController(
        IUserAuthService userAuthService,
        IServiceAccountAuthService serviceAccountAuthService)
    {
        _userAuthService = userAuthService;
        _serviceAccountAuthService = serviceAccountAuthService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req, CancellationToken ct)
    {
        var result = await _userAuthService.LoginAsync(req.Username, req.Password, ct);

        if (result == null)
            return Unauthorized();

        return Ok(new
        {
            token = result.Token,
            result.Username,
            result.Role
        });
    }

    [HttpPost("service-login")]
    [AllowAnonymous]
    public async Task<IActionResult> ServiceLogin(ServiceLoginRequest request, CancellationToken ct)
    {
        var result = await _serviceAccountAuthService.LoginAsync(request.ApiKey, ct);

        if (result == null)
            return Unauthorized("Invalid API key.");

        return Ok(new
        {
            token = result.Token,
            AppName = result.AppName
        });
    }
}