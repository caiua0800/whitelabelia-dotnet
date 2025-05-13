namespace backend.Interfaces;

public interface IAgentService
{
    Task<int?> GetEnterpriseIdByAgentNumberAsync(string agentNumber);
}
