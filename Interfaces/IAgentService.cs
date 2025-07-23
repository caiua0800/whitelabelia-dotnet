using backend.Models;

namespace backend.Interfaces;

public interface IAgentService
{
    Task<int?> GetEnterpriseIdByAgentNumberAsync(string agentNumber);
    Task<List<Agent>?> GetAgentsByEnterpriseId();
    Task<Agent?> GetAgent(int enterprise_id);
    Task<string?> GetPrompt(string? agentNumber);
    Task<bool> UpdatePrompt(string newPrompt, string agentNumber);
    Task<string?> GetPrompt1(string agentNumber);
    

}
