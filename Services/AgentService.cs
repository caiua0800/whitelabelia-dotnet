// IAgentService.cs
namespace backend.Services;

using backend.Interfaces;
using backend.Models;
using Microsoft.EntityFrameworkCore;

public class AgentService : IAgentService
{
    private readonly ApplicationDbContext _context;

    public AgentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int?> GetEnterpriseIdByAgentNumberAsync(string agentNumber)
    {
        var agent = await _context.Agents
            .Where(a => a.Number == agentNumber)
            .FirstOrDefaultAsync();

        return agent?.EnterpriseId;
    }

    public async Task<string?> GetPrompt(string agentNumber)
    {
        var agent = await _context.Agents
            .Where(a => a.Number == agentNumber)
            .FirstOrDefaultAsync();

        return agent?.Prompt;
    }
}