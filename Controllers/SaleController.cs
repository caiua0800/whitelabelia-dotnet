using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;
using backend.DTOs;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using backend.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaleController : ControllerBase
{
    private readonly SaleService _saleService;
    private readonly IAgentService _agentService;

    public SaleController(SaleService saleService, IAgentService agentService)
    {
        _saleService = saleService;
        _agentService = agentService;
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

    [HttpPost("enterprise/{agentNumberId}")]
    public async Task<ActionResult<SaleWithProductsDto>> Create([FromBody] CreateSaleDto createSaleDto, string agentNumberId)
    {
        try
        {
            var enterpriseId = await _agentService.GetEnterpriseIdByAgentNumberAsync(agentNumberId);
            Console.WriteLine($"enterpriseId2: {enterpriseId}");

            var createdSale = await _saleService.CreateSaleAsync(createSaleDto, (int)enterpriseId);

            var result = new SaleWithProductsDto(
                createdSale.Sale,
                createdSale.Products,
                createdSale.PaymentInfo,
                enterpriseId
            );

            return CreatedAtAction(nameof(GetById), new { id = createdSale.Sale.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/new-status/{newStatus}")]
    public async Task<ActionResult<Sale>> UpdateStatus(
            int id,
            int newStatus)
    {
        try
        {
            var updatedSale = await _saleService.UpdateSaleStatusAsync(id, newStatus);

            if (updatedSale == null)
            {
                return NotFound("Venda não encontrada");
            }

            return Ok(updatedSale);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }
}

