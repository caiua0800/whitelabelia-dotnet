// IAgentService.cs
namespace backend.Services;

using System.Globalization;
using System.Text;
using backend.Models;
using Microsoft.EntityFrameworkCore;

public class SubscriptionService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly SubscriptionTypeService _subscriptionTypeService;
    private readonly EnterpriseService _enterpriseService;

    public SubscriptionService(ApplicationDbContext context, ITenantService tenantService, SubscriptionTypeService subscriptionTypeService, EnterpriseService enterpriseService)
    {
        _context = context;
        _tenantService = tenantService;
        _subscriptionTypeService = subscriptionTypeService;
        _enterpriseService = enterpriseService;
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var subscriptions = await _context.Subscriptions
            .Where(a => a.EnterpriseId == enterpriseId)
            .ToListAsync();

        return subscriptions;
    }

    public async Task<IEnumerable<SubscriptionDto>> GetSubscriptionsDtoAsync()
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var subscriptions = await _context.Subscriptions
            .Where(a => a.EnterpriseId == enterpriseId)
            .ToListAsync();

        List<SubscriptionDto> subscriptionsDto = new List<SubscriptionDto>();

        foreach (var subs in subscriptions)
        {
            if (subs.SubscriptionTypeId != null)
            {
                var subType = await _subscriptionTypeService.GetSubscriptionTypeById((int)subs.SubscriptionTypeId);
                if (subType != null)
                {
                    var enterprise = await _enterpriseService.GetEnterpriseByIdAsync((int)subs.EnterpriseId);
                    if (enterprise != null)
                    {
                        subscriptionsDto.Add(new SubscriptionDto(subs, subType, enterprise));
                    }
                }
            }
        }

        return subscriptionsDto;
    }

    public async Task<Subscription?> GetSubscriptionById(int id)
    {
        var subscription = await _context.Subscriptions
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync();

        return subscription;
    }

    public async Task<Subscription?> GetSubscriptionByEnterpriseId(int id)
    {
        var subscription = await _context.Subscriptions
            .Where(a => a.EnterpriseId == id)
            .FirstOrDefaultAsync();

        return subscription;
    }

    public async Task<Subscription> CreateSubscriptionAsync(Subscription subscription)
    {
        subscription.DateCreated = DateTime.Now;

        var existingSubscriptionType = await _subscriptionTypeService.GetSubscriptionTypeById((int)subscription.SubscriptionTypeId);

        if (existingSubscriptionType == null)
            return null;

        subscription.AvaliableShots = existingSubscriptionType.ShotsQtt;
        subscription.AvaliableStartChats = existingSubscriptionType.StartChatsQtt;
        subscription.AvaliableUsers = existingSubscriptionType.UsersQtt;

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }

    public async Task<Subscription> HandlePaySubscription(int subscriptionId)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();

        var subscription = await _context.Subscriptions
            .Where(s => s.Id == subscriptionId && s.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        if (subscription == null)
        {
            throw new KeyNotFoundException("Subscription not found");
        }

        var existingSubscriptionType = await _subscriptionTypeService
            .GetSubscriptionTypeById((int)subscription.SubscriptionTypeId );

        if (existingSubscriptionType == null)
        {
            throw new KeyNotFoundException("Subscription type not found");
        }

        subscription.Status = 2; 
        subscription.AvaliableShots = existingSubscriptionType.ShotsQtt;
        subscription.AvaliableStartChats = existingSubscriptionType.StartChatsQtt;
        subscription.DatePaid = DateTime.Now;

        if (existingSubscriptionType.Duration.HasValue)
        {
            subscription.ExpirationDate = DateTime.Now.AddDays(existingSubscriptionType.Duration.Value);
        }

        _context.Subscriptions.Update(subscription);
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

    public async Task<Subscription> UpdateSubscriptionAsync(Subscription subscription)
    {
        var enterpriseId = _tenantService.GetCurrentEnterpriseId();
        var existingSubscription = await _context.Subscriptions
            .Where(p => p.Id == subscription.Id && p.EnterpriseId == enterpriseId)
            .FirstOrDefaultAsync();

        if (existingSubscription == null)
        {
            throw new KeyNotFoundException("Product not found");
        }

        existingSubscription = subscription;

        _context.Subscriptions.Update(existingSubscription);
        await _context.SaveChangesAsync();

        return existingSubscription;
    }
}