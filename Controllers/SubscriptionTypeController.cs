using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionTypeController : ControllerBase
{
    private readonly SubscriptionTypeService _subscriptionTypeService;

    public SubscriptionTypeController(SubscriptionTypeService subscriptionTypeService)
    {
        _subscriptionTypeService = subscriptionTypeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubscriptionType>>> GetAll()
    {
        var subscriptions = await _subscriptionTypeService.GetSubscriptionsTypeAsync();
        return Ok(subscriptions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubscriptionType>> GetById(int id)
    {
        var product = await _subscriptionTypeService.GetSubscriptionTypeById(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Subscription>> Create([FromBody] SubscriptionType subscriptionType)
    {
        try
        {
            var createdProduct = await _subscriptionTypeService.CreateSubscriptionTypeAsync(subscriptionType);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SubscriptionType subscriptionType)
    {
        if (id != subscriptionType.Id)
            return BadRequest("ID da subscription não corresponde");

        try
        {
            var updatedCategory = await _subscriptionTypeService.UpdateSubscriptionTypeAsync(subscriptionType);
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