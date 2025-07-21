// IAgentService.cs
namespace backend.Services;

using System.Globalization;
using System.Text;
using backend.Models;
using Microsoft.EntityFrameworkCore;

public class SubscriptionTypeService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public SubscriptionTypeService(ApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<IEnumerable<SubscriptionType>> GetSubscriptionsTypeAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var subscriptions = await _context.SubscriptionTypes            .ToListAsync();

        return subscriptions;
    }

    public async Task<SubscriptionType?> GetSubscriptionTypeById(int id)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var subscription = await _context.SubscriptionTypes
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync();

        return subscription;
    }


    public async Task<SubscriptionType> CreateSubscriptionTypeAsync(SubscriptionType subscription)
    {
        subscription.DateCreated = DateTime.Now;
        _context.SubscriptionTypes.Add(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }

    private static string RemoveAccents(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        text = text.ToLower();
        text = text.Normalize(NormalizationForm.FormD);
        var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).Normalize(NormalizationForm.FormC);
    }

    public async Task<SubscriptionType> UpdateSubscriptionTypeAsync(SubscriptionType subscription)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var existingSubscription = await _context.SubscriptionTypes
            .Where(p => p.Id == subscription.Id)
            .FirstOrDefaultAsync();

        if (existingSubscription == null)
        {
            throw new KeyNotFoundException("Product not found");
        }

        existingSubscription = subscription;

        _context.SubscriptionTypes.Update(existingSubscription);
        await _context.SaveChangesAsync();

        return existingSubscription;
    }
}