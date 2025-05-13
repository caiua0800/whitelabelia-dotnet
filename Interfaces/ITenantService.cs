using backend.Models;
using Microsoft.AspNetCore.Http;

namespace backend.Services;

public interface ITenantService
{
    Enterprise? GetCurrentEnterprise();
    int? TryGetCurrentEnterpriseId(); 
    int GetCurrentEnterpriseId();
    string GetPrimaryColor();
    string GetBrandName();
    string? GetLogoUrl();
}

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public Enterprise? GetCurrentEnterprise()
    {
        return _httpContextAccessor.HttpContext?.Items["CurrentEnterprise"] as Enterprise;
    }
    
    public int? TryGetCurrentEnterpriseId()
    {
        return GetCurrentEnterprise()?.Id;
    }
    
    public int GetCurrentEnterpriseId()
    {
        return TryGetCurrentEnterpriseId() ?? throw new InvalidOperationException("Tenant não identificado");
    }
    
    public string GetPrimaryColor()
    {
        return GetCurrentEnterprise()?.PrimaryColor ?? "#4f46e5";
    }
    
    public string GetBrandName()
    {
        return GetCurrentEnterprise()?.BrandName ?? GetCurrentEnterprise()?.Name ?? "MyApp";
    }
    
    public string? GetLogoUrl()
    {
        return GetCurrentEnterprise()?.LogoUrl;
    }
}