using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;


namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnterpriseController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly ICredentialGeneratorService _credentialGenerator;

    public EnterpriseController(
        ApplicationDbContext context,
        ITenantService tenantService,
        ICredentialGeneratorService credentialGenerator)
    {
        _context = context;
        _tenantService = tenantService;
        _credentialGenerator = credentialGenerator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Enterprise>>> GetEnterprises()
    {
        return await _context.Enterprises.IgnoreQueryFilters().ToListAsync();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyEnterprise()
    {
        var enterprise = HttpContext.Items["CurrentEnterprise"] as Enterprise;

        Console.WriteLine($"Enterprise recebida: {enterprise.Id}");

        if (enterprise == null)
        {
            return Unauthorized(new { message = "Empresa não encontrada no contexto." });
        }

        return Ok(enterprise);
    }


    // GET: api/enterprises/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Enterprise>> GetEnterprise(int id)
    {
        var enterprise = await _context.Enterprises.FindAsync(id);

        if (enterprise == null)
        {
            return NotFound();
        }

        return enterprise;
    }

    [HttpPost("with-admin")]
    public async Task<ActionResult<EnterpriseWithAdminResponse>> CreateEnterpriseWithAdmin(
    [FromBody] CreateEnterpriseWithAdminRequest request)
    {
        // Validar CNPJ único
        if (await _context.Enterprises.AnyAsync(e => e.Cnpj == request.Cnpj))
        {
            return BadRequest(new { message = "CNPJ já cadastrado" });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {

            // Criar Enterprise
            var enterprise = new Enterprise
            {
                Name = request.EnterpriseName,
                Cnpj = request.Cnpj,
                Contact = request.Contact,
                Email = request.Email,
                DateCreated = DateTime.UtcNow,
                Status = 1,
                PrimaryColor = request.PrimaryColor ?? "#4f46e5",
                SecondaryColor = request.SecondaryColor ?? "#6366f1",
                Domain = request.Domain?.ToLower()
            };

            // Adicionar Enterprise
            _context.Enterprises.Add(enterprise);
            await _context.SaveChangesAsync();

            var address = new Address
            {
                Street = request.Address.Street,
                Number = request.Address.Number,
                Neighborhood = request.Address.Neighborhood,
                City = request.Address.City,
                State = request.Address.State,
                Country = request.Address.Country,
                Zipcode = request.Address.Zipcode,
                EnterpriseId = enterprise.Id
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            // Criar Admin padrão com permissões básicas
            var loginId = _credentialGenerator.GenerateRandomLoginId();
            var password = _credentialGenerator.GenerateRandomPassword();

            var admin = new Admin
            {
                Name = request.AdminName,
                LoginId = loginId,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                EnterpriseId = enterprise.Id,
                Permissions = new List<AdminPermission>
            {
                new() { Name = "manage_settings", Allowed = true },
                new() { Name = "view_dashboard", Allowed = true }
            }
            };

            // Adicionar Admin
            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            // Commit da transação
            await transaction.CommitAsync();

            // Retornar sucesso
            return Ok(new EnterpriseWithAdminResponse
            {
                Enterprise = enterprise,
                AdminCredentials = new AdminCredentials
                {
                    LoginId = loginId,
                    Password = password
                }
            });
        }
        catch (Exception ex)
        {
            // Caso ocorra algum erro, fazer rollback da transação
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Erro ao criar empresa", error = ex.Message });
        }
    }


    // POST: api/enterprises
    [HttpPost]
    public async Task<ActionResult<Enterprise>> PostEnterprise(Enterprise enterprise)
    {
        _context.Enterprises.Add(enterprise);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetEnterprise", new { id = enterprise.Id }, enterprise);
    }

    // PUT: api/enterprises/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutEnterprise(int id, Enterprise enterprise)
    {
        if (id != enterprise.Id)
        {
            return BadRequest();
        }

        _context.Entry(enterprise).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EnterpriseExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    private bool EnterpriseExists(int id)
    {
        return _context.Enterprises.Any(e => e.Id == id);
    }
}
