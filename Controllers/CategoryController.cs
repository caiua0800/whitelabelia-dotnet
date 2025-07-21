using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoryController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll()
    {
        var categories = await _categoryService.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetById(int id)
    {
        var product = await _categoryService.GetCategoryById(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Category>> Create([FromBody] Category product)
    {
        try
        {
            var createdProduct = await _categoryService.CreateCategoryAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Category category)
    {
        if (id != category.Id)
            return BadRequest("ID da categoria não corresponde");

        try
        {
            var updatedCategory = await _categoryService.UpdateCategoryAsync(category);
            return Ok(updatedCategory);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}