// Middlewares/TenantMiddleware.cs
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace backend.Middlewares;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        Enterprise enterprise = null;

        var enterpriseId = GetEnterpriseIdFromToken(context);

        if (enterpriseId.HasValue)
        {
            enterprise = await dbContext.Enterprises.FindAsync(enterpriseId.Value);
        }

        context.Items["CurrentEnterprise"] = enterprise;
        await _next(context);
    }

    private int? GetEnterpriseIdFromToken(HttpContext context)
    {

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
            return null;

        var token = authHeader.Substring("Bearer ".Length).Trim();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;


        if (jwtToken == null)
            return null;

        var enterpriseIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "EnterpriseId");

        if (enterpriseIdClaim == null)
            return null;


        if (int.TryParse(enterpriseIdClaim.Value, out int enterpriseId))
        {
            return enterpriseId;
        }

        return null;
    }
}
