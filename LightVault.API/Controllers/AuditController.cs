using LightVault.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LightVault.API.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Roles = "Admin,Auditor")]
public class AuditController : ControllerBase
{
    private readonly IAuditQueryService _auditQueryService;
    private readonly IAuditVerificationService _auditVerificationService;

    public AuditController(
        IAuditQueryService auditQueryService,
        IAuditVerificationService auditVerificationService)
    {
        _auditQueryService = auditQueryService;
        _auditVerificationService = auditVerificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var list = await _auditQueryService.GetAllAsync(ct);
        return Ok(list);
    }

    [HttpGet("filter")]
    public async Task<IActionResult> Filter(
        [FromQuery] string? user,
        [FromQuery] string? action,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var result = await _auditQueryService.FilterAsync(user, action, from, to, ct);
        return Ok(result);
    }

    [HttpGet("verify")]
    public async Task<IActionResult> Verify(CancellationToken ct = default)
    {
        var result = await _auditVerificationService.VerifyAsync(ct);
        return Ok(result);
    }
}