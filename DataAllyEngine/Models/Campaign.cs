using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("campaign")]
[Index("ChannelId", Name = "Campaign_Channel_FK")]
public partial class Campaign
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("channel_id")]
    public int ChannelId { get; set; }

    [Column("channel_campaign_id")]
    [StringLength(255)]
    public string ChannelCampaignId { get; set; } = null!;

    [Column("name")]
    [StringLength(1024)]
    public string Name { get; set; } = null!;

    [Column("objective")]
    [StringLength(255)]
    public string? Objective { get; set; }

    [Column("status")]
    [StringLength(255)]
    public string? Status { get; set; }

    [Column("attribution_setting")]
    public int? AttributionSetting { get; set; }

    [Column("budget")]
    [Precision(10, 4)]
    public decimal? Budget { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("updated", TypeName = "datetime")]
    public DateTime? Updated { get; set; }

    [InverseProperty("Campaign")]
    public virtual ICollection<Adset> Adsets { get; set; } = new List<Adset>();

    [ForeignKey("ChannelId")]
    [InverseProperty("Campaigns")]
    public virtual Channel Channel { get; set; } = null!;
}
