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

    public async Task<Agent> CreateAgentAsync(Agent agent)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        agent.DateCreated = DateTime.Now;
        agent.EnterpriseId = enterpriseId;
        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();
        return agent;
    }


    public async Task<Agent?> GetAgent(int enterprise_id)
    {
        var agent = await _context.Agents
            .Where(a => a.EnterpriseId == enterprise_id)
            .FirstOrDefaultAsync();

        return agent;
    }

        public async Task<Agent?> GetAgentById(int id)
    {
        var agent = await _context.Agents
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync();

        return agent;
    }

    public async Task<List<Agent>?> GetAgentsByEnterpriseId()
    {

        var enterpriseId = _tenantService.TryGetCurrentEnterpriseId();

        var agents = await _context.Agents
            .Where(a => a.EnterpriseId == enterpriseId)
            .ToListAsync();

        return agents;
    }


    public async Task<string?> GetPrompt(string agentNumber)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        var agent = await _context.Agents
            .Where(a => a.EnterpriseId == enterpriseId && a.Number == agentNumber)
            .FirstOrDefaultAsync();

        return agent?.Prompt;
    }

    public async Task<bool> UpdatePrompt(string newPrompt, string agentNumber)
{
    var agent = await _context.Agents
        .Where(a => a.Number == agentNumber)
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