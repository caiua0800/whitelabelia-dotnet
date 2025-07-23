using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IChatService _chatService;

    public MessageController(IMessageService messageService, IChatService chatService)
    {
        _messageService = messageService;
        _chatService = chatService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetMessage(int id)
    {
        var message = await _messageService.GetMessageByIdAsync(id);

        if (message == null)
        {
            return NotFound();
        }

        return message;
    }

    [HttpGet("chat/{chatId}/{agentNumber}")]
    public async Task<ActionResult<IEnumerable<Message>>> GetMessagesByChatId(string chatId, string agentNumber)
    {
        var messages = await _messageService.GetMessagesByChatIdAsync(chatId, agentNumber);

        if (messages == null || !messages.Any())
        {
            return NotFound();
        }

        return Ok(messages);
    }

    [HttpPost]
    public async Task<ActionResult<Message>> CreateMessage([FromBody] Message message)
    {
        if (message.ChatId == null) return BadRequest("ChatId é obrigatório");

        var createdMessage = await _messageService.SendMessageAsync(message);
        return CreatedAtAction(nameof(GetMessage), new { id = createdMessage.Id }, createdMessage);
    }

    // PUT: api/messages/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutMessage(int id, Message message)
    {
        if (id != message.Id)
        {
            return BadRequest();
        }

        try
        {
            await _messageService.UpdateMessageAsync(message);
        }
        catch
        {
            return NotFound();
        }

        return NoContent();
    }

    
}