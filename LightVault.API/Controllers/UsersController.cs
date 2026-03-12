using LightVault.API.DTOs;
using LightVault.Domain.Interfaces;
using LightVault.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LightVault.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var user = await _userManagementService.GetAsync(id, ct);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _userManagementService.GetAllAsync(ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest req, CancellationToken ct)
    {
        var result = await _userManagementService.CreateAsync(
            req.Username,
            req.Password,
            req.Role,
            ct);

        if (result.Status == CreateUserStatus.UsernameAlreadyExists)
        {
            return BadRequest(new
            {
                message = "Username already exists"
            });
        }

        if (result.Status == CreateUserStatus.Error)
        {
            return BadRequest(new
            {
                message = "User is not created. System error!"
            });
        }

        return Ok(new
        {
            result.Id,
            result.Username,
            result.Role,
            result.IsActive
        });
    }

    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, UpdateUserRoleRequest req, CancellationToken ct)
    {
        var result = await _userManagementService.UpdateRoleAsync(id, req.Role, ct);

        if (result == UpdateUserRoleStatus.NotFound)
            return NotFound();

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _userManagementService.DeleteAsync(id, ct);

        if (result == DeleteUserStatus.NotFound)
            return NotFound();

        return Ok();
    }
}