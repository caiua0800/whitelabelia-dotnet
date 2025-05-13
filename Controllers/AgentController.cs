using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly AgentService _agentService;

    public AgentController(AgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpGet]
    public async Task<ActionResult<string>> GetAgentPrompt([FromQuery] string agentNumber)
    {
        var agentPrompt = await _agentService.GetPrompt(agentNumber);
        return Ok(agentPrompt);
    }
}