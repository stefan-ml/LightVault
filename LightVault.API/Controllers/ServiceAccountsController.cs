using LightVault.API.DTOs;
using LightVault.Domain.Interfaces;
using LightVault.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LightVault.API.Controllers;

[ApiController]
[Route("api/service-accounts")]
[Authorize(Roles = "Admin")]
public class ServiceAccountsController : ControllerBase
{
    private readonly IServiceAccountService _serviceAccountService;

    public ServiceAccountsController(IServiceAccountService serviceAccountService)
    {
        _serviceAccountService = serviceAccountService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateServiceAccountRequest request,
        CancellationToken ct)
    {
        var result = await _serviceAccountService.CreateAsync(
            request.AppName,
            request.Role,
            ct);

        if (result.Status == CreateServiceAccountStatus.AlreadyExists)
            return BadRequest("Service account already exists.");

        return Ok(new
        {
            result.Id,
            result.AppName,
            result.Role,
            result.ApiKey
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _serviceAccountService.GetAllAsync(ct);
        return Ok(list);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        var result = await _serviceAccountService.RevokeAsync(id, ct);

        if (result == RevokeServiceAccountStatus.NotFound)
            return NotFound();

        return NoContent();
    }
}