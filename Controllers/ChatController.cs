using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IChatExportService _exportService;

    public ChatController(IChatService chatService, IChatExportService exportService)
    {
        _chatService = chatService;
        _exportService = exportService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatWithLastMessageDto>>> GetAll()
    {
        var chats = await _chatService.GetAllChatsWithLastMessageAsync();
        return Ok(chats);
    }

    [HttpPost("multiple")]
    public async Task<IActionResult> CreateMultipleChats([FromBody] List<MultipleChat> chats)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdCount = await _chatService.CreateMultipleChatsAsync(chats);
            return Ok(new { Message = $"{createdCount} chats criados com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<ChatDto>>> SearchChats(
    [FromQuery] string? searchTerm,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? order = "desc",
    [FromQuery] DateTime? startDate = null,
    [FromQuery] DateTime? endDate = null,
    [FromQuery] string? tagIds = null,
    [FromQuery] bool? withMessage = false) // Agora recebe como string
    {
        Console.WriteLine($"withMessage: {withMessage}");
        try
        {
            List<int> tagIdsList = new List<int>();

            if (!string.IsNullOrEmpty(tagIds))
            {
                // Converte a string separada por vírgulas em List<int>
                tagIdsList = tagIds.Split(',')
                                  .Select(idStr => int.TryParse(idStr, out int id) ? id : (int?)null)
                                  .Where(id => id.HasValue)
                                  .Select(id => id.Value)
                                  .ToList();
            }

            var result = await _chatService.SearchChatsAsync(
                searchTerm,
                pageNumber,
                pageSize,
                order,
                startDate,
                endDate,
                tagIdsList.Any() ? tagIdsList : null,
                withMessage); // Passa null se a lista estiver vazia

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ocorreu um erro interno: {ex.Message}");
        }
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
        Console.WriteLine("aqui");
        try
        {
            var res = await _chatService.UpdateChatAsync(chat);
            return Ok(res);
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

    [HttpPut("tag/{id}")]
    public async Task<IActionResult> UpdateChatTags(string id, [FromBody] List<int> newTags)
    {
        try
        {
            var res = await _chatService.UpdateChatTagsAsync(id, newTags);
            return Ok(res);
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

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportToExcel()
    {
        var content = await _exportService.ExportChatsToExcelAsync();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "tabela-clientes.xlsx");
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportToPdf()
    {
        var content = await _exportService.ExportChatsToPdfAsync();
        return File(content, "application/pdf", "tabela-clientes.pdf");
    }


    [HttpGet("export/excel/advanced")]
    public async Task<IActionResult> ExportToExcelAdvanced()
    {
        var content = await _exportService.ExportChatsToExcelAdvancedAsync();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "clientes-avancado.xlsx");
    }

}

public class UpdateCustomPromptDto
{
    public string Id { get; set; }
    public string Text { get; set; }
}
