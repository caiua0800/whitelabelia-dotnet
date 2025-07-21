using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;

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

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Shot>>> Search(
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

    [HttpPost("send/{id}")]
    public async Task<IActionResult> SendShot(int id, [FromBody] List<ClientShotDto> clients)
    {
        try
        {
            await _shotService.SendShot(id, clients);
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
    public async Task<IActionResult> SendShot([FromBody] ClientShotDtoStart dto)
    {
        try
        {
            await _shotService.SendStartChatShot(dto.ClientShotDto, dto.TextToSend, dto.MyName);
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
