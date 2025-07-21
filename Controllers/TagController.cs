using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagController : ControllerBase
{
    private readonly TagService _tagService;

    public TagController(TagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tag>>> GetAllTags()
    {
        var tags = await _tagService.GetAllTagsAsync();
        return Ok(tags);
    }

    [HttpGet("enterprise/{id}")]
    public async Task<ActionResult<IEnumerable<Tag>>> GetAllTags(int id)
    {
        var tags = await _tagService.GetTagsByEnterpriseIdAsync(id);
        return Ok(tags);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tag>> GetByIdTag(int id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);
        if (tag == null) return NotFound();
        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<Chat>> CreateTag([FromBody] Tag tag)
    {
        try
        {
            var createdTag = await _tagService.CreateTagAsync(tag);
            return CreatedAtAction(nameof(GetByIdTag), new { id = createdTag.Id }, createdTag);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] Tag tag)
    {
        if (tag == null)
            return BadRequest("Dados da tag não fornecidos");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != tag.Id)
            return BadRequest("ID da tag não corresponde");

        try
        {
            var updatedTag = await _tagService.UpdateTagAsync(tag);
            return Ok(updatedTag);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao atualizar a tag");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        try
        {
            await _tagService.DeleteTagAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao excluir a tag");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Tag>>> SearchTagsByName([FromQuery] string name)
    {
        try
        {
            var tags = await _tagService.SearchTagsByNameAsync(name);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocorreu um erro ao buscar as tags.", details = ex.Message });
        }
    }
}