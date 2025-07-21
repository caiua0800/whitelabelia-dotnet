using backend.Models;

namespace backend.Interfaces;

public interface IAgentService
{
    Task<int?> GetEnterpriseIdByAgentNumberAsync(string agentNumber);
    Task<Agent?> GetAgent(int enterprise_id);
    Task<string?> GetPrompt();
    Task<bool> UpdatePrompt(string newPrompt);
    Task<string?> GetPrompt1(string agentNumber);
    

}
