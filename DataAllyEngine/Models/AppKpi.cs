using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("appkpi")]
[Index("AdId", Name = "AppKpi_Ad_FK")]
public partial class AppKpi
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ad_id")]
    public int AdId { get; set; }

    [Column("effective_date", TypeName = "datetime")]
    public DateTime EffectiveDate { get; set; }

    [Column("installs")]
    public int? Installs { get; set; }

    [Column("installs_rate")]
    [Precision(10)]
    public decimal? InstallsRate { get; set; }

    [Column("app_open")]
    public int? AppOpen { get; set; }

    [Column("conversion_rate")]
    public float? ConversionRate { get; set; }

    [Column("cost_per_app_install")]
    [Precision(10, 4)]
    public decimal? CostPerAppInstall { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("updated", TypeName = "datetime")]
    public DateTime? Updated { get; set; }

    [ForeignKey("AdId")]
    [InverseProperty("Appkpis")]
    public virtual Ad Ad { get; set; } = null!;
}
