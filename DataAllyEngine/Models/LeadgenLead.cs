using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("leadgenlead")]
[Index("LeadgenkpiId", Name = "LeadGenLead_LeadGen_FK")]
[Index("LeadgenkpiId", Name = "leadgenkpi_id_UNIQUE", IsUnique = true)]
public partial class LeadgenLead
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("leadgenkpi_id")]
    public int LeadgenkpiId { get; set; }

    [Column("leads")]
    public int? Leads { get; set; }

    [Column("cost_per_lead")]
    [Precision(10, 4)]
    public decimal? CostPerLead { get; set; }

    [Column("channel_lead_gen_forms_submitted")]
    public int? ChannelLeadGenFormsSubmitted { get; set; }

    [Column("website_leads")]
    public int? WebsiteLeads { get; set; }

    [Column("website_leads_value")]
    [Precision(10, 4)]
    public decimal? WebsiteLeadsValue { get; set; }

    [Column("cost_per_website_lead")]
    [Precision(10, 4)]
    public decimal? CostPerWebsiteLead { get; set; }

    [ForeignKey("LeadgenkpiId")]
    public virtual LeadgenKpi LeadgenKpi { get; set; } = null!;
}
