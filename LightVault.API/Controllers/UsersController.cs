using LightVault.Infrastructure.Data;
using LightVault.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace LightVault.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly LightVaultDbContext _db;

    public UsersController(LightVaultDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var user = await _db.Users
            .Where(u => u.Id == id)
            .Select(u => new
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role,
                IsActive = u.IsActive
            })
            .FirstOrDefaultAsync();

        if (user == null) return NotFound();
        return Ok(user);
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Users
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Role,
                u.IsActive
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest req)
    {
        var exists = await _db.Users
        .AnyAsync(u => u.Username.ToLower() == req.Username.ToLower());

        if (exists)
        {
            return BadRequest(new
            {
                message = "Username already exists"
            });
        }

        var hash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(req.Password)));

        var user = new UserEntity
        {
            Username = req.Username,
            PasswordHash = hash,
            Role = req.Role,
            IsActive = true
        };

        try
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new
            {
                message = "User is not created. System error!"
            });
        }

        return Ok(user);
    }

    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, UpdateUserRoleRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.Role = req.Role;
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok();
    }
}

public class CreateUserRequest
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string Role { get; set; } = "Developer";
}

public class UpdateUserRoleRequest
{
    public string Role { get; set; } = default!;
}
