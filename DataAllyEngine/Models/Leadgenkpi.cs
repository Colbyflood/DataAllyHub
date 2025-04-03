using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("leadgenkpi")]
[Index("AdId", Name = "LeadGenKpi_Ad_FK")]
[Index("AdId", "EffectiveDate", Name = "leadgenkpi_UK", IsUnique = true)]
public partial class Leadgenkpi
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ad_id")]
    public int AdId { get; set; }

    [Column("effective_date", TypeName = "datetime")]
    public DateTime? EffectiveDate { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("updated", TypeName = "datetime")]
    public DateTime? Updated { get; set; }

    [ForeignKey("AdId")]
    [InverseProperty("Leadgenkpis")]
    public virtual Ad Ad { get; set; } = null!;

    [InverseProperty("Leadgenkpi")]
    public virtual Leadgenapplication? Leadgenapplication { get; set; }

    [InverseProperty("Leadgenkpi")]
    public virtual Leadgenappointment? Leadgenappointment { get; set; }

    [InverseProperty("Leadgenkpi")]
    public virtual Leadgencontact? Leadgencontact { get; set; }

    [InverseProperty("Leadgenkpi")]
    public virtual Leadgenlead? Leadgenlead { get; set; }

    [InverseProperty("Leadgenkpi")]
    public virtual Leadgenlocation? Leadgenlocation { get; set; }

    [InverseProperty("Leadgenkpi")]
    public virtual Leadgenregistration? Leadgenregistration { get; set; }

    [InverseProperty("Leadgenkpi")]
    public virtual Leadgensubscription? Leadgensubscription { get; set; }

    [InverseProperty("Leadgenkpi")]
    public virtual Leadgentrial? Leadgentrial { get; set; }
}
