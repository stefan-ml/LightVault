using LightVault.API.DTOs;
using LightVault.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LightVault.API.Controllers;

[ApiController]
[Route("api/secrets")]
[Authorize]
public class SecretsController : ControllerBase
{
    private readonly ISecretService _secretService;

    public SecretsController(ISecretService secretService)
    {
        _secretService = secretService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var list = await _secretService.GetAllAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
    {
        var result = await _secretService.GetAsync(id, ct);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSecretRequest req, CancellationToken ct = default)
    {
        var id = await _secretService.CreateAsync(req.Name, req.Value, ct);
        return Ok(id);
    }

    [HttpGet("{id:guid}/versions/{version:int}")]
    public async Task<IActionResult> GetByVersion(Guid id, int version, CancellationToken ct = default)
    {
        var result = await _secretService.GetByVersionAsync(id, version, ct);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("{id:guid}/rotate")]
    public async Task<IActionResult> Rotate(Guid id, RotateSecretRequest req, CancellationToken ct = default)
    {
        try
        {
            var result = await _secretService.RotateAsync(id, req.NewValue, ct);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [Authorize(Roles = "Admin,Developer,ServiceClient")]
    [HttpGet("by-name/{name}")]
    public async Task<IActionResult> GetByName(string name, CancellationToken ct = default)
    {
        var result = await _secretService.GetByNameAsync(name, ct);
        if (result == null)
            return NotFound();

        return Ok(result);
    }
}