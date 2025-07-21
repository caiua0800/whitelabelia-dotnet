using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await _productService.GetProductsAsync();
        return Ok(products);
    }

    [HttpGet("agent/{agentNumber}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetAllByAgentNumber(string agentNumber)
    {
        Console.WriteLine("GetAllByAgentNumber");
        var products = await _productService.GetProductsByAgentNumberAsync(agentNumber);
        return Ok(products);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<Product>>> SearchProducts(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? order = "desc",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] List<string>? categoryNames = null,
        [FromQuery] int? status = null)
    {
        try
        {

            var result = await _productService.SearchProductsAsync(
                searchTerm,
                pageNumber,
                pageSize,
                order,
                startDate,
                endDate,
                categoryNames,
                status);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ocorreu um erro interno: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _productService.GetProductById(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] Product product)
    {
        try
        {
            product.CategoryNames = product.CategoryNames?.ToArray() ?? Array.Empty<string>();

            var createdProduct = await _productService.CreateProductAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("status/{id}")]
    public async Task<ActionResult<bool>> GetProductStatusById(int id)
    {
        var product = await _productService.GetProductById(id);
        if (product == null) return NotFound();

        return product.Status == 1;
    }

    [HttpPut("status")]
    public async Task<IActionResult> UpdateStatus([FromQuery] int id, [FromQuery] int newStatus)
    {
        try
        {
            var product = await _productService.GetProductById(id);
            if (product == null) return NotFound();

            product.Status = newStatus;
            await _productService.UpdateProductAsync(product);
            return Ok("Status alterado.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ocorreu um erro interno: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> UpdateProduct(int id, [FromBody] Product product)
    {
        try
        {
            if (id != product.Id)
            {
                return BadRequest("ID do produto não corresponde à rota.");
            }

            product.CategoryNames = product.CategoryNames?.ToArray() ?? Array.Empty<string>();

            var updatedProduct = await _productService.UpdateProductAsync(product);
            return Ok(updatedProduct);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }
}