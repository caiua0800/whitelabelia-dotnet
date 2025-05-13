// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;
        private readonly ITenantService _tenantService;

        public AuthController(ApplicationDbContext context, AuthService authService, ITenantService tenantService)
        {
            _context = context;
            _authService = authService;
            _tenantService = tenantService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.LoginId == request.LoginId);

            if (admin == null || !BCrypt.Net.BCrypt.Verify(request.Password, admin.Password))
            {
                return Unauthorized(new { message = "Login ou senha inválidos" });
            }

            var accessToken = _authService.GenerateAccessToken(admin);
            var refreshToken = _authService.GenerateRefreshToken();

            // Salvar o refresh token e a data de expiração no banco de dados
            admin.RefreshToken = refreshToken;
            admin.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);  // Expiração em 7 dias
            await _context.SaveChangesAsync();

            return Ok(new
            {
                access_token = accessToken,
                refresh_token = refreshToken,
                admin = new
                {
                    admin.Id,
                    admin.Name,
                    admin.LoginId,
                    admin.Email
                }
            });
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.RefreshToken == request.RefreshToken);

            if (admin == null || admin.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Refresh token inválido ou expirado" });
            }

            var newAccessToken = _authService.GenerateAccessToken(admin);
            var newRefreshToken = _authService.GenerateRefreshToken();

            // Atualizar refresh token no banco de dados
            admin.RefreshToken = newRefreshToken;
            admin.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Expiração em 7 dias
            await _context.SaveChangesAsync();

            return Ok(new
            {
                access_token = newAccessToken,
                refresh_token = newRefreshToken
            });
        }


    }
}
