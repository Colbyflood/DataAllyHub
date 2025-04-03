using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("adset")]
[Index("CampaignId", Name = "Adset_Campaign_FK")]
public partial class Adset
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("campaign_id")]
    public int CampaignId { get; set; }

    [Column("channel_adset_id")]
    [StringLength(255)]
    public string ChannelAdsetId { get; set; } = null!;

    [Column("name")]
    [StringLength(1024)]
    public string Name { get; set; } = null!;

    [Column("status")]
    [StringLength(255)]
    public string? Status { get; set; }

    [Column("adset_created", TypeName = "datetime")]
    public DateTime? AdsetCreated { get; set; }

    [Column("adset_updated", TypeName = "datetime")]
    public DateTime? AdsetUpdated { get; set; }

    [Column("spend")]
    [Precision(10, 4)]
    public decimal? Spend { get; set; }

    [Column("attribution_setting")]
    public int? AttributionSetting { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("updated", TypeName = "datetime")]
    public DateTime? Updated { get; set; }

    [InverseProperty("Adset")]
    public virtual ICollection<Ad> Ads { get; set; } = new List<Ad>();

    [InverseProperty("Adset")]
    public virtual ICollection<Adsetad> Adsetads { get; set; } = new List<Adsetad>();

    [ForeignKey("CampaignId")]
    [InverseProperty("Adsets")]
    public virtual Campaign Campaign { get; set; } = null!;
}
