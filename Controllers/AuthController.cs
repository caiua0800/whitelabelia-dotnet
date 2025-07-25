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
        private readonly EnterpriseService _enterpriseService;
        private readonly SubscriptionService _subscriptionService;

        public AuthController(ApplicationDbContext context, AuthService authService, ITenantService tenantService, EnterpriseService enterpriseService, SubscriptionService subscriptionService)
        {
            _context = context;
            _authService = authService;
            _tenantService = tenantService;
            _enterpriseService = enterpriseService;
            _subscriptionService = subscriptionService;
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
            Enterprise enterprise = await _enterpriseService.GetEnterpriseByIdAsync(admin.EnterpriseId);
            var enterprise_subscription = await _subscriptionService.GetSubscriptionByEnterpriseId(admin.EnterpriseId);

            admin.RefreshToken = refreshToken;
            admin.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
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
                    admin.Email,
                    admin.DateCreated,
                    admin.Permissions,
                },
                enterprise,
                subscription_info = new
                {
                    expiration = enterprise_subscription?.ExpirationDate,
                    status = enterprise_subscription?.Status,
                    block = enterprise_subscription?.Status == 3, 
                    block_date = enterprise_subscription?.ExpirationDate?.AddDays(3), 
                    block_days_remaining = enterprise_subscription?.ExpirationDate != null ?
                    DiasRestantesParaTresDias(enterprise_subscription.ExpirationDate.Value) : (int?)null
                }
            });
        }

        public static int DiasRestantesParaTresDias(DateTime dataOriginal)
        {
            DateTime dataComTresDias = dataOriginal.AddDays(3);
            TimeSpan diferenca = dataComTresDias - DateTime.Now;
            return (int)Math.Ceiling(diferenca.TotalDays);
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
