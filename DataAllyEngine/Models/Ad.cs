using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("ad")]
[Index("AdsetId", Name = "ad_adset_FK_idx")]
[Index("AssetId", Name = "ad_asset_fk_idx")]
public partial class Ad
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("adset_id")]
    public int AdsetId { get; set; }

    [Column("channel_ad_id")]
    [StringLength(255)]
    public string ChannelAdId { get; set; } = null!;

    [Column("asset_id")]
    public int AssetId { get; set; }

    [Column("name")]
    [StringLength(1024)]
    public string Name { get; set; } = null!;

    [Column("dataally_name")]
    [StringLength(1024)]
    public string DataallyName { get; set; } = null!;

    [Column("ad_created", TypeName = "datetime")]
    public DateTime? AdCreated { get; set; }

    [Column("ad_updated", TypeName = "datetime")]
    public DateTime? AdUpdated { get; set; }

    [Column("ad_deactivated", TypeName = "datetime")]
    public DateTime? AdDeactivated { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("updated", TypeName = "datetime")]
    public DateTime? Updated { get; set; }

    [ForeignKey("AdsetId")]
    [InverseProperty("Ads")]
    public virtual Adset Adset { get; set; } = null!;

    [InverseProperty("Ad")]
    public virtual ICollection<Adsetad> Adsetads { get; set; } = new List<Adsetad>();

    [InverseProperty("Ad")]
    public virtual ICollection<Appkpi> Appkpis { get; set; } = new List<Appkpi>();

    [InverseProperty("Ad")]
    public virtual ICollection<Ecommercekpi> Ecommercekpis { get; set; } = new List<Ecommercekpi>();

    [InverseProperty("Ad")]
    public virtual ICollection<Generalkpi> Generalkpis { get; set; } = new List<Generalkpi>();

    [InverseProperty("Ad")]
    public virtual ICollection<Leadgenkpi> Leadgenkpis { get; set; } = new List<Leadgenkpi>();

    [InverseProperty("Ad")]
    public virtual ICollection<Videokpi> Videokpis { get; set; } = new List<Videokpi>();
}
