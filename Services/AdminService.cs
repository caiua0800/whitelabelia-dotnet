using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AdminService
{
    private readonly ApplicationDbContext _context;

    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Admin>> GetAllAdmins()
    {
        return await _context.Admins.ToListAsync();
    }

    public async Task<Admin?> GetAdminById(string id)
    {
        return await _context.Admins.FindAsync(id);
    }

    public async Task<Admin> CreateAdmin(Admin admin)
    {
        if (await _context.Admins.AnyAsync(c => c.Email == admin.Email))
        {
            throw new Exception("Já existe um admin com este Email");
        }
        
        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();
        return admin;
    }

    public async Task UpdateAdmin(Admin admin)
    {
        _context.Entry(admin).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAdmin(string email)
    {
        var admin = await _context.Admins.FindAsync(email);
        if (admin != null)
        {
            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
        }
    }
}