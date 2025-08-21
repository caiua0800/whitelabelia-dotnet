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
    private readonly AgentService _agentService;

    public EnterpriseService(ApplicationDbContext context, AgentService agentService)
    {
        _context = context;
        _agentService = agentService;
    }

    public async Task<Enterprise?> GetEnterpriseByIdAsync(int id)
    {
        return await _context.Enterprises
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
    }
    public async Task<Enterprise?> GetEnterpriseByAgentNumberAsync(string number)
    {
        var ent_id = await _agentService.GetEnterpriseIdByAgentNumberAsync(number);

        return await _context.Enterprises
            .Where(c => c.Id == ent_id)
            .FirstOrDefaultAsync();
    }

}