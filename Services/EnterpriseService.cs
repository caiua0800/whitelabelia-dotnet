using backend.Models;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using System.Globalization;

namespace backend.Services;


public class EnterpriseService
{
    private readonly ApplicationDbContext _context;

    public EnterpriseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Enterprise?> GetEnterpriseByIdAsync(int id)
    {
        return await _context.Enterprises
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
    }


}