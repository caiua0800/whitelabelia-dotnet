// Controllers/ClientController.cs
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Admin>>> Get()
    {
        return await _adminService.GetAllAdmins();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Admin>> Get(string id)
    {
        var admin = await _adminService.GetAdminById(id);
        if (admin == null)
        {
            return NotFound();
        }
        return admin;
    }

    [HttpPost]
    public async Task<ActionResult<Admin>> Post(Admin admin)
    {
        try
        {
            var createdAdmin = await _adminService.CreateAdmin(admin);
            return CreatedAtAction(nameof(Get), new { email = createdAdmin.Email }, createdAdmin);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{email}")]
    public async Task<IActionResult> Put(string email, Admin admin)
    {
        if (email != admin.Email)
        {
            return BadRequest("ID do admin não corresponde");
        }

        try
        {
            await _adminService.UpdateAdmin(admin);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{email}")]
    public async Task<IActionResult> Delete(string email)
    {
        try
        {
            await _adminService.DeleteAdmin(email);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}