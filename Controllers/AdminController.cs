// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;


namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;

        public AdminController(ApplicationDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // Listar todos os Admins
        [HttpGet]
        public async Task<IActionResult> GetAllAdmins()
        {
            var enterpriseId = _tenantService.GetCurrentEnterpriseId();

            var admins = await _context.Admins.Where(t => t.EnterpriseId == enterpriseId).ToListAsync();
            return Ok(admins);
        }

        // Buscar Admin por Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdminById(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }
            return Ok(admin);
        }

        [HttpPut("change-password/{id}")]
        [Authorize]
        public async Task<IActionResult> ChangeAdminPassword(int id, [FromBody] ChangePasswordRequest request)
        {
            var enterpriseId = _tenantService.GetCurrentEnterpriseId();

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Id == id && a.EnterpriseId == enterpriseId);

            if (admin == null)
            {
                return NotFound(new { message = "Administrador não encontrado ou não pertence a esta empresa" });
            }

            // 3. Validar a nova senha
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            {
                return BadRequest(new { message = "A nova senha deve ter pelo menos 6 caracteres" });
            }

            // 4. Se for requisição do próprio usuário (não admin), verificar a senha atual
            var currentAdminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (currentAdminId == id && !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, admin.Password))
            {
                return Unauthorized(new { message = "Senha atual incorreta" });
            }

            // 5. Atualizar a senha
            admin.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Senha alterada com sucesso" });
        }

        // Criar novo Admin
        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] Admin admin)
        {
            // Exemplo: Criptografar a senha ao criar
            admin.Password = BCrypt.Net.BCrypt.HashPassword(admin.Password);

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAdminById), new { id = admin.Id }, admin);
        }

        // Atualizar Admin
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdmin(int id, [FromBody] Admin updatedAdmin)
        {
            if (id != updatedAdmin.Id)
            {
                return BadRequest();
            }

            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            admin.Name = updatedAdmin.Name;
            admin.LoginId = updatedAdmin.LoginId;
            admin.Email = updatedAdmin.Email;

            if (!string.IsNullOrEmpty(updatedAdmin.Password))
            {
                admin.Password = BCrypt.Net.BCrypt.HashPassword(updatedAdmin.Password);
            }

            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Deletar Admin
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("by-enterprise")]
        [Authorize]
        public async Task<IActionResult> GetAdminsByEnterprise()
        {
            var enterprise = HttpContext.Items["CurrentEnterprise"] as Enterprise;

            if (enterprise == null)
            {
                return Unauthorized(new { message = "Empresa não encontrada no contexto." });
            }

            var admins = await _context.Admins
                .Where(a => a.EnterpriseId == enterprise.Id)
                .ToListAsync();

            return Ok(admins);
        }

    }
}
