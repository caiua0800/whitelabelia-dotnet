using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly SubscriptionService _subscriptionService;

    public SubscriptionController(SubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Subscription>>> GetAll()
    {
        var subscriptions = await _subscriptionService.GetSubscriptionsAsync();
        return Ok(subscriptions);
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetAll2()
    {
        var subscriptions = await _subscriptionService.GetSubscriptionsDtoAsync();
        return Ok(subscriptions);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<Subscription>> GetById(int id)
    {
        var product = await _subscriptionService.GetSubscriptionById(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpGet("ticket")]
    public async Task<ActionResult<string>> GetTicketById(int id)
    {
        var product = await _subscriptionService.GetSubscriptionTicketId();
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Subscription>> Create([FromBody] Subscription subscription)
    {
        try
        {
            var exist = await _subscriptionService.GetSubscriptionByEnterpriseId(subscription.EnterpriseId);
            if (exist != null) return Forbid("Já existe uma incrição.");
            var createdProduct = await _subscriptionService.CreateSubscriptionAsync(subscription);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Subscription subscription)
    {
        if (id != subscription.Id)
            return BadRequest("ID da subscription não corresponde");

        try
        {
            var updatedCategory = await _subscriptionService.UpdateSubscriptionAsync(subscription);
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

    [HttpPost("pay/{subscriptionId}")]
    public async Task<IActionResult> HandlePaySubscription(int subscriptionId)
    {
        try
        {
            var updatedSubscription = await _subscriptionService.HandlePaySubscription(subscriptionId);
            return Ok(updatedSubscription);
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