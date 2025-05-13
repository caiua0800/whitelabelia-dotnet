using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatWithLastMessageDto>>> GetAll()
    {
        var chats = await _chatService.GetAllChatsWithLastMessageAsync();
        return Ok(chats);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Chat>> GetById(string id)
    {
        var chat = await _chatService.GetChatByIdAsync(id);
        if (chat == null) return NotFound();
        return Ok(chat);
    }

    [HttpGet("{id}/custom-prompt")]
    public async Task<ActionResult<string?>> GetCustomPromptByChatId(string id)
    {
        var prompt = await _chatService.GetCustomPromptByChatId(id);
        if (prompt == null) return Ok("");
        return Ok(prompt);
    }



    [HttpGet("status/{id}")]
    public async Task<ActionResult<bool>> GetChatStatusById(string id)
    {
        var chat = await _chatService.GetChatByIdAsync(id);
        if (chat == null) return NotFound();

        return chat.Status == 1;
    }

    [HttpGet("{id}/last-message-seen")]
    public async Task<ActionResult<bool?>> GetLastMessageIsSeen(string id)
    {
        var res = await _chatService.GetLastMessageIsSeenAsync(id);
        if (res == null) return false;

        return res;
    }


    [HttpPost]
    public async Task<ActionResult<Chat>> Create([FromBody] Chat chat)
    {
        try
        {
            var createdChat = await _chatService.CreateChatAsync(chat);
            return CreatedAtAction(nameof(GetById), new { id = createdChat.Id }, createdChat);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Chat chat)
    {
        if (id != chat.Id) return BadRequest("ID do chat não corresponde");

        try
        {
            await _chatService.UpdateChatAsync(chat);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpPut("status")]
    public async Task<IActionResult> UpdateStatus([FromQuery] string id, [FromQuery] int newStatus)
    {
        try
        {
            await _chatService.UpdateChatStatusAsync(id, newStatus);
            return Ok("Status alterado.");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpPut("custom-prompt")]
    public async Task<IActionResult> UpdateCustomPrompt([FromBody] UpdateCustomPromptDto dto)
    {
        try
        {
            await _chatService.UpdateCustomPrompt(dto.Id, dto.Text);
            return Ok("Custom prompt alterado.");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpPut("seen")]
    public async Task<IActionResult> UpdateSeen([FromQuery] string id)
    {
        try
        {
            await _chatService.UpdateChatLastMessageIsSeenAsync(id);
            return Ok("Mensagem colocado como vista.");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _chatService.DeleteChatAsync(id);
            return NoContent();
        }
        catch (Exception)
        {
            return NotFound();
        }
    }
}

public class UpdateCustomPromptDto
{
    public string Id { get; set; }
    public string Text { get; set; }
}
