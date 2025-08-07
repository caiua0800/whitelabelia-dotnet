using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;
using backend.DTOs;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageModelController : ControllerBase
{
    private readonly MessageModelService _messageModelService;

    public MessageModelController(MessageModelService messageModelService)
    {
        _messageModelService = messageModelService;
    }

    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<MessageModel>>> GetMessageModels()
    {
        var messages = await _messageModelService.GetMessageModels();
        return Ok(messages);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MessageModel>> GetMessageModelById(int id)
    {
        var message = await _messageModelService.GetMessageModelByIdAsync(id);
        
        if (message == null)
        {
            return NotFound();
        }
        
        return Ok(message);
    }

    [HttpPost]
    public async Task<ActionResult<MessageModel>> CreateMessageModel([FromBody] MessageModelCreateDto createDto)
    {
        try
        {
            var createdModel = await _messageModelService.CreateMessageModelAsync(createDto);
            return CreatedAtAction(
                nameof(GetMessageModelById), 
                new { id = createdModel.Id },
                createdModel);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { 
                error = ex.Message,
                type = "validation"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                error = ex.Message,
                details = ex.InnerException?.Message,
                type = "api_error"
            });
        }
    }
}