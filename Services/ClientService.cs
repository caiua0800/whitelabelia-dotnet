using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ClientService
{
    private readonly ApplicationDbContext _context;

    public ClientService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Client>> GetAllClients()
    {
        return await _context.Clients.ToListAsync();
    }

    public async Task<Client?> GetClientById(string id)
    {
        return await _context.Clients.FindAsync(id);
    }

    public async Task<Client> CreateClient(Client client)
    {
        // Verifica se já existe um cliente com esse ID
        if (await _context.Clients.AnyAsync(c => c.Id == client.Id))
        {
            throw new Exception("Já existe um cliente com este ID");
        }
        
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task UpdateClient(Client client)
    {
        _context.Entry(client).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteClient(string id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client != null)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }
    }
}