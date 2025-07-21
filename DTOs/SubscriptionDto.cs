using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class SubscriptionDto
{
    [Column("subscription")]
    public Subscription Subscription { get; set; }

    [Column("subscription_type")]
    public SubscriptionType SubscriptionType { get; set; }

    [Column("enterprise")]
    public Enterprise Enterprise { get; set; }

    public SubscriptionDto(Subscription subscription, SubscriptionType subscriptionType, Enterprise enterprise)
    {
        Subscription = subscription;
        SubscriptionType = subscriptionType;
        Enterprise = enterprise;
    }
}