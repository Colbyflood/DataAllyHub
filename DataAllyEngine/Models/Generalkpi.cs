using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("generalkpi")]
[Index("AdId", Name = "GeneralKpi_Ad_FK")]
[Index("AdId", "EffectiveDate", Name = "generalkpi_UK", IsUnique = true)]
public partial class Generalkpi
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ad_id")]
    public int AdId { get; set; }

    [Column("effective_date", TypeName = "datetime")]
    public DateTime EffectiveDate { get; set; }

    [Column("is_active", TypeName = "bit(1)")]
    public ulong IsActive { get; set; }

    [Column("spend")]
    [Precision(10, 4)]
    public decimal? Spend { get; set; }

    [Column("frequency")]
    [Precision(10)]
    public decimal? Frequency { get; set; }

    [Column("reach")]
    public int? Reach { get; set; }

    [Column("impressions")]
    public int? Impressions { get; set; }

    [Column("quality_ranking")]
    [StringLength(255)]
    public string? QualityRanking { get; set; }

    [Column("engagement_ranking")]
    [StringLength(255)]
    public string? EngagementRanking { get; set; }

    [Column("conversion_rate_ranking")]
    [StringLength(255)]
    public string? ConversionRateRanking { get; set; }

    [Column("all_clicks")]
    public int? AllClicks { get; set; }

    [Column("link_click_total")]
    public int? LinkClickTotal { get; set; }

    [Column("outbound_clicks")]
    public int? OutboundClicks { get; set; }

    [Column("all_ctr")]
    [Precision(10)]
    public decimal? AllCtr { get; set; }

    [Column("outbound_ctr")]
    [Precision(10)]
    public decimal? OutboundCtr { get; set; }

    [Column("all_cpc")]
    [Precision(10, 4)]
    public decimal? AllCpc { get; set; }

    [Column("outbound_link_click_cpc")]
    [Precision(10, 4)]
    public decimal? OutboundLinkClickCpc { get; set; }

    [Column("cpm")]
    [Precision(10)]
    public decimal? Cpm { get; set; }

    [Column("ad_recall_lift")]
    public int? AdRecallLift { get; set; }

    [Column("ad_recall_rate")]
    [Precision(10)]
    public decimal? AdRecallRate { get; set; }

    [Column("page_likes")]
    public int? PageLikes { get; set; }

    [Column("page_engagements")]
    public int? PageEngagements { get; set; }

    [Column("post_comments")]
    public int? PostComments { get; set; }

    [Column("post_shares")]
    public int? PostShares { get; set; }

    [Column("post_saves")]
    public int? PostSaves { get; set; }

    [Column("post_reactions")]
    public int? PostReactions { get; set; }

    [Column("photos_viewed")]
    public int? PhotosViewed { get; set; }

    [Column("cost_per_link_click")]
    [Precision(10, 4)]
    public decimal? CostPerLinkClick { get; set; }

    [Column("inline_ctr")]
    [Precision(10, 4)]
    public decimal? InlineCtr { get; set; }

    [Column("landing_page_view")]
    public int? LandingPageView { get; set; }

    [Column("website_view_content")]
    public int? WebsiteViewContent { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("updated", TypeName = "datetime")]
    public DateTime? Updated { get; set; }

    [ForeignKey("AdId")]
    [InverseProperty("Generalkpis")]
    public virtual Ad Ad { get; set; } = null!;
}
