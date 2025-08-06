using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;
using backend.DTOs;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShotController : ControllerBase
{
    private readonly ShotService _shotService;

    public ShotController(ShotService shotService)
    {
        _shotService = shotService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Shot>>> GetAll()
    {
        var chats = await _shotService.GetAllShotsAsync();
        return Ok(chats);
    }

    [HttpGet("dto")]
    public async Task<ActionResult<IEnumerable<ShotWithMessageModelDto>>> GetAllDto()
    {
        var chats = await _shotService.GetAllShotsDtoAsync();
        return Ok(chats);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ShotWithMessageModelDto>>> Search(
    [FromQuery] string? searchTerm,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string order = "desc",
    [FromQuery] string? startDate = null,
    [FromQuery] string? endDate = null,
    [FromQuery] int? status = null)
    {
        var shots = await _shotService.SearchShotsAsync(
            searchTerm,
            pageNumber,
            pageSize,
            order,
            startDate,
            endDate,
            status);
        return Ok(shots);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] Shot shot)
    {
        try
        {
            var createdProduct = await _shotService.CreateChatAsync(shot);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("check-template-status/{shotId}")]
    public async Task<IActionResult> CheckTemplateStatus(int shotId)
    {
        try
        {
            // Obter o shot pelo ID
            var shot = await _shotService.GetShotByIdAsync(shotId);
            if (shot == null)
            {
                return NotFound(new { success = false, message = "Disparo não encontrado" });
            }

            if (shot.MessageModelId == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Este disparo não possui um modelo de mensagem associado"
                });
            }


            await _shotService.CheckAndUpdateTemplateStatus(shotId);

            return Ok(new
            {
                success = true,
                message = "Status do template verificado com sucesso"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Erro ao verificar status do template",
                details = ex.Message
            });
        }
    }

    [HttpPost("send/{id}")]
    public async Task<IActionResult> SendShot(int id, [FromBody] List<ClientShotDto> clients, [FromQuery] string agentNumber)
    {
        try
        {
            await _shotService.SendShot(id, agentNumber, clients);
            return Ok("Mensagens Enviadas com sucesso");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("monthly-stats")]
    public async Task<ActionResult<ShotMonthlyStatsDto>> GetMonthlyStats(
    [FromQuery] int month,
    [FromQuery] int year,
    [FromHeader(Name = "Authorization")] string authToken)
    {
        try
        {

            if (string.IsNullOrEmpty(authToken))
            {
                return Unauthorized("Token de autenticação é necessário");
            }

            var stats = await _shotService.GetMonthlyStatsAsync(month, year);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("send2/{id}")]
    public async Task<IActionResult> SendShotStartingLeads(int id, [FromBody] List<ClientShotDto> clients, [FromQuery] string agentNumber)
    {
        try
        {
            await _shotService.SendShotStartingLeads(id, agentNumber, clients);
            return Ok("Mensagens Enviadas com sucesso");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("start-chat")]
    public async Task<IActionResult> SendShot([FromBody] ClientShotDtoStart dto, [FromQuery] string agentNumber)
    {
        try
        {
            await _shotService.SendStartChatShot(dto.ClientShotDto, dto.TextToSend, dto.MyName, agentNumber);
            return Ok("Chat iniciado com sucesso.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("start-chat-leads")]
    public async Task<IActionResult> SendShotLeads([FromBody] ClientShotDtoStartLeads dto, [FromQuery] string agentNumber)
    {
        try
        {
            await _shotService.SendStartChatLeadsShot(dto.ClientShotDto, agentNumber);
            return Ok("Chat iniciado com sucesso.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Shot>> GetById(int id)
    {
        var shot = await _shotService.GetShotByIdAsync(id);
        if (shot == null) return NotFound();
        return Ok(shot);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _shotService.DeleteShotAsync(id);
            return NoContent();
        }
        catch (Exception)
        {
            return NotFound();
        }
    }
}
