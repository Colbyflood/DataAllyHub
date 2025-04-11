using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("leadgensubscription")]
[Index("LeadgenkpiId", Name = "LeadGenSubscription_LeadGen_FK")]
[Index("LeadgenkpiId", Name = "leadgenkpi_id_UNIQUE", IsUnique = true)]
public partial class LeadgenSubscription
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("leadgenkpi_id")]
    public int LeadgenkpiId { get; set; }

    [Column("subscriptions")]
    public int? Subscriptions { get; set; }

    [Column("subscriptions_value")]
    [Precision(10, 4)]
    public decimal? SubscriptionsValue { get; set; }

    [Column("cost_per_subscriptions")]
    [Precision(10, 4)]
    public decimal? CostPerSubscriptions { get; set; }

    [Column("mobile_app_subscriptions")]
    public int? MobileAppSubscriptions { get; set; }

    [Column("mobile_app_subscriptions_value")]
    [Precision(10, 4)]
    public decimal? MobileAppSubscriptionsValue { get; set; }

    [Column("cost_per_mobile_app_subscriptions")]
    [Precision(10, 4)]
    public decimal? CostPerMobileAppSubscriptions { get; set; }

    [Column("website_subscriptions")]
    public int? WebsiteSubscriptions { get; set; }

    [Column("website_subscriptions_value")]
    [Precision(10, 4)]
    public decimal? WebsiteSubscriptionsValue { get; set; }

    [Column("cost_per_website_subscriptions")]
    [Precision(10, 4)]
    public decimal? CostPerWebsiteSubscriptions { get; set; }

    [ForeignKey("LeadgenkpiId")]
    public virtual LeadgenKpi LeadgenKpi { get; set; } = null!;
}
