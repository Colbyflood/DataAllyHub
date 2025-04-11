using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("leadgenkpi")]
[Index("AdId", Name = "LeadGenKpi_Ad_FK")]
[Index("AdId", "EffectiveDate", Name = "leadgenkpi_UK", IsUnique = true)]
public partial class LeadgenKpi
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

    [InverseProperty("LeadgenKpi")]
    public virtual LeadgenApplication? Leadgenapplication { get; set; }

    [InverseProperty("LeadgenKpi")]
    public virtual LeadgenAppointment? Leadgenappointment { get; set; }

    [InverseProperty("LeadgenKpi")]
    public virtual LeadgenContact? Leadgencontact { get; set; }

    [InverseProperty("LeadgenKpi")]
    public virtual LeadgenLead? Leadgenlead { get; set; }

    [InverseProperty("LeadgenKpi")]
    public virtual LeadgenLocation? Leadgenlocation { get; set; }

    [InverseProperty("LeadgenKpi")]
    public virtual LeadgenRegistration? Leadgenregistration { get; set; }

    [InverseProperty("LeadgenKpi")]
    public virtual LeadgenSubscription? Leadgensubscription { get; set; }

    [InverseProperty("LeadgenKpi")]
    public virtual LeadgenTrial? Leadgentrial { get; set; }
}
