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
    public async Task<ActionResult<string>> GetAgentPrompt1([FromQuery] string agentNumber)
    {
        var agentPrompt = await _agentService.GetPrompt1(agentNumber);
        return Ok(agentPrompt);
    }

    [HttpGet("prompt")]
    public async Task<ActionResult<string?>> GetAgentPrompt()
    {
        var agentPrompt = await _agentService.GetPrompt();
        return Ok(agentPrompt);
    }

    [HttpPut("prompt")]
    public async Task<IActionResult> UpdateAgentPrompt([FromBody] UpdatePromptDto updatePromptDto)
    {
        if (string.IsNullOrWhiteSpace(updatePromptDto.NewPrompt))
        {
            return BadRequest("O prompt não pode ser vazio");
        }

        var success = await _agentService.UpdatePrompt(updatePromptDto.NewPrompt);
        
        if (!success)
        {
            return NotFound("Agente não encontrado para esta empresa");
        }

        return Ok();
    }
}

// DTO para a atualização do prompt
public class UpdatePromptDto
{
    public string NewPrompt { get; set; } = string.Empty;
}