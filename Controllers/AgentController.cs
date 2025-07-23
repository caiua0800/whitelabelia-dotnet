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

    [HttpPost]
    public async Task<ActionResult<Category>> Create([FromBody] Agent agent)
    {
        try
        {
            var createdItem = await _agentService.CreateAgentAsync(agent);
            return CreatedAtAction(nameof(GetById), new { id = createdItem.Id }, createdItem);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<Agent>> GetById([FromQuery] int id)
    {
        var agentPrompt = await _agentService.GetAgentById(id);
        return Ok(agentPrompt);
    }

    [HttpGet("agent-prompt")]
    public async Task<ActionResult<string>> GetAgentPrompt1([FromQuery] string agentNumber)
    {
        var agentPrompt = await _agentService.GetPrompt1(agentNumber);
        return Ok(agentPrompt);
    }

    [HttpGet("all")]
    public async Task<List<Agent>?> GetAgentsByEnterpriseId()
    {
        return await _agentService.GetAgentsByEnterpriseId();
    }

    [HttpGet("prompt")]
    public async Task<ActionResult<string?>> GetAgentPrompt([FromQuery] string? agentNumber)
    {
        var agentPrompt = await _agentService.GetPrompt(agentNumber);
        return Ok(agentPrompt);
    }

    [HttpPut("prompt")]
    public async Task<IActionResult> UpdateAgentPrompt([FromBody] UpdatePromptDto updatePromptDto)
    {
        if (string.IsNullOrWhiteSpace(updatePromptDto.NewPrompt))
        {
            return BadRequest("O prompt não pode ser vazio");
        }

        if (string.IsNullOrWhiteSpace(updatePromptDto.AgentNumber))
        {
            return BadRequest("Número do agente não especificado");
        }

        var success = await _agentService.UpdatePrompt(updatePromptDto.NewPrompt, updatePromptDto.AgentNumber);

        if (!success)
        {
            return NotFound("Agente não encontrado");
        }

        return Ok();
    }
}

public class UpdatePromptDto
{
    public string NewPrompt { get; set; } = string.Empty;
    public string AgentNumber { get; set; } = string.Empty;
}