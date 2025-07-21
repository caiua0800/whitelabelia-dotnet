// IAgentService.cs
namespace backend.Services;
using backend.Models;
using Microsoft.EntityFrameworkCore;

public class AgentPromptService
{
    private readonly ApplicationDbContext _context;

    public AgentPromptService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetAgentPrompt(int enterpriseId)
    {
        var agent = await _context.AgentPrompts
            .Where(a => a.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        return agent?.Text;
    }
}