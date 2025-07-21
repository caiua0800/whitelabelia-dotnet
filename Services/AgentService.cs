namespace backend.Services;

using backend.Interfaces;
using backend.Models;
using Microsoft.EntityFrameworkCore;

public class AgentService : IAgentService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public AgentService(ApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<int?> GetEnterpriseIdByAgentNumberAsync(string agentNumber)
    {
        var agent = await _context.Agents
            .Where(a => a.Number == agentNumber)
            .FirstOrDefaultAsync();

        return agent?.EnterpriseId;
    }

    public async Task<Agent?> GetAgent(int enterprise_id)
    {
        var agent = await _context.Agents
            .Where(a => a.EnterpriseId == enterprise_id)
            .FirstOrDefaultAsync();

        return agent;
    }

    public async Task<string?> GetPrompt()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        var agent = await _context.Agents
            .Where(a => a.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        return agent?.Prompt;
    }

    public async Task<bool> UpdatePrompt(string newPrompt)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var agent = await _context.Agents
            .Where(a => a.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        if (agent == null)
        {
            return false;
        }

        agent.Prompt = newPrompt;
        _context.Agents.Update(agent);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<string?> GetPrompt1(string agentNumber)
    {
        var agent = await _context.Agents
            .Where(a => a.Number == agentNumber)
            .FirstOrDefaultAsync();

        return agent?.Prompt;
    }
}