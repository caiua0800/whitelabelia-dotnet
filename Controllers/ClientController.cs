// Controllers/ClientController.cs
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly ClientService _clientService;

    public ClientController(ClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Client>>> Get()
    {
        return await _clientService.GetAllClients();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> Get(string id)
    {
        var client = await _clientService.GetClientById(id);
        if (client == null)
        {
            return NotFound();
        }
        return client;
    }

    [HttpPost]
    public async Task<ActionResult<Client>> Post(Client client)
    {
        try
        {
            var createdClient = await _clientService.CreateClient(client);
            return CreatedAtAction(nameof(Get), new { id = createdClient.Id }, createdClient);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, Client client)
    {
        if (id != client.Id)
        {
            return BadRequest("ID do cliente não corresponde");
        }

        try
        {
            await _clientService.UpdateClient(client);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _clientService.DeleteClient(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}