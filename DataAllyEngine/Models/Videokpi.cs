using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("videokpi")]
[Index("AdId", Name = "VideoKpi_Ad_FK")]
[Index("AdId", "EffectiveDate", Name = "videokpi_UK", IsUnique = true)]
public partial class Videokpi
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ad_id")]
    public int AdId { get; set; }

    [Column("effective_date", TypeName = "datetime")]
    public DateTime EffectiveDate { get; set; }

    [Column("play_3seconds")]
    public int? Play3seconds { get; set; }

    [Column("play_thru")]
    public int? PlayThru { get; set; }

    [Column("play_total")]
    public int? PlayTotal { get; set; }

    [Column("play_25percent")]
    public int? Play25percent { get; set; }

    [Column("play_50percent")]
    public int? Play50percent { get; set; }

    [Column("play_75percent")]
    public int? Play75percent { get; set; }

    [Column("play_95percent")]
    public int? Play95percent { get; set; }

    [Column("play_100percent")]
    public int? Play100percent { get; set; }

    [Column("average_watch_seconds")]
    public int? AverageWatchSeconds { get; set; }

    [Column("watched_30seconds")]
    public int? Watched30seconds { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("updated", TypeName = "datetime")]
    public DateTime? Updated { get; set; }

    [ForeignKey("AdId")]
    [InverseProperty("Videokpis")]
    public virtual Ad Ad { get; set; } = null!;
}
