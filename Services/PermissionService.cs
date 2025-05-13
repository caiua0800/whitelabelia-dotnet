namespace backend.Services;
using backend.Models;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> CheckPermissionAsync(int adminId, string permissionName)
    {
        var admin = await _context.Admins
            .Where(a => a.Id == adminId)
            .Select(a => new { a.Permissions })
            .FirstOrDefaultAsync();

        return admin?.Permissions.Any(p => p.Name == permissionName && p.Allowed) ?? false;
    }

    // Versão síncrona para uso em filters/attributes
    public bool HasPermission(string permissionName)
    {
        var admin = _httpContextAccessor.HttpContext?.Items["Admin"] as Admin;
        return admin?.Permissions.Any(p => p.Name == permissionName && p.Allowed) ?? false;
    }
}