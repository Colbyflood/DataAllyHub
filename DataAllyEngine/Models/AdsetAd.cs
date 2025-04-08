using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("adsetad")]
[Index("AdId", Name = "AdsetAd_Ad_FK")]
[Index("AdsetId", Name = "AdsetAd_Adset_FK")]
public partial class AdsetAd
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("adset_id")]
    public int AdsetId { get; set; }

    [Column("ad_id")]
    public int AdId { get; set; }

    [ForeignKey("AdId")]
    [InverseProperty("Adsetads")]
    public virtual Ad Ad { get; set; } = null!;

    [ForeignKey("AdsetId")]
    [InverseProperty("Adsetads")]
    public virtual Adset Adset { get; set; } = null!;
}
