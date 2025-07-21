using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;
using backend.DTOs;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaleController : ControllerBase
{
    private readonly SaleService _saleService;

    public SaleController(SaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sale>>> GetAll()
    {
        var sales = await _saleService.GetAllSalesAsync();
        return Ok(sales);
    }

    [HttpGet("dto")]
    public async Task<ActionResult<List<SaleWithProductsDto>>> GetAllWithDto()
    {
        var sales = await _saleService.GetAllSaleWithProductsAsync();
        return Ok(sales);
    }

    [HttpGet("dto/{id}")]
    public async Task<ActionResult<SaleWithProductsDto>> GetByIdWithProductDto(int id)
    {
        var sale = await _saleService.GetSaleByIdWithProductsAsync(id);
        if (sale == null) return NotFound();
        return Ok(sale);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Sale>> GetById(int id)
    {
        var sale = await _saleService.GetSaleByIdAsync(id);
        if (sale == null) return NotFound();
        return Ok(sale);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<SaleWithProductsDto>>> SearchSales(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string order = "desc",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? status = null)
    {
        try
        {
            var result = await _saleService.SearchSalesAsync(
                searchTerm,
                pageNumber,
                pageSize,
                order,
                startDate,
                endDate,
                status);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("enterprise/{enterpriseId}")]
    public async Task<ActionResult<SaleWithProductsDto>> Create([FromBody] CreateSaleDto createSaleDto, int enterpriseId)
    {
        try
        {
            Console.WriteLine($"enterpriseId2: {enterpriseId}");
            var createdSale = await _saleService.CreateSaleAsync(createSaleDto, enterpriseId);
            return CreatedAtAction(nameof(GetById), new { id = createdSale.Sale.Id }, createdSale);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
