using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("ecommercekpi")]
[Index("AdId", Name = "EcommerceKpi_Ad_FK")]
[Index("AdId", "EffectiveDate", Name = "ecommercekpi_UK", IsUnique = true)]
public partial class Ecommercekpi
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ad_id")]
    public int AdId { get; set; }

    [Column("effective_date", TypeName = "datetime")]
    public DateTime EffectiveDate { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("updated", TypeName = "datetime")]
    public DateTime? Updated { get; set; }

    [ForeignKey("AdId")]
    [InverseProperty("Ecommercekpis")]
    public virtual Ad Ad { get; set; } = null!;

    [InverseProperty("Ecommercekpi")]
    public virtual Ecommercechannel? Ecommercechannel { get; set; }

    [InverseProperty("Ecommercekpi")]
    public virtual Ecommercetotal? Ecommercetotal { get; set; }

    [InverseProperty("Ecommercekpi")]
    public virtual Ecommercewebsite? Ecommercewebsite { get; set; }
}
