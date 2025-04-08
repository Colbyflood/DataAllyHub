using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("leadgentrial")]
[Index("LeadgenkpiId", Name = "LeadGenTrial_LeadGen_FK")]
[Index("LeadgenkpiId", Name = "leadgenkpi_id_UNIQUE", IsUnique = true)]
public partial class LeadgenTrial
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("leadgenkpi_id")]
    public int LeadgenkpiId { get; set; }

    [Column("total_trials_started")]
    public int? TotalTrialsStarted { get; set; }

    [Column("total_trials_started_value")]
    [Precision(10, 4)]
    public decimal? TotalTrialsStartedValue { get; set; }

    [Column("mobile_trials_started")]
    public int? MobileTrialsStarted { get; set; }

    [Column("mobile_trials_started_value")]
    [Precision(10, 4)]
    public decimal? MobileTrialsStartedValue { get; set; }

    [Column("website_trials_started")]
    public int? WebsiteTrialsStarted { get; set; }

    [Column("website_trials_started_value")]
    [Precision(10, 4)]
    public decimal? WebsiteTrialsStartedValue { get; set; }

    [Column("offline_trials_started")]
    public int? OfflineTrialsStarted { get; set; }

    [Column("offline_trials_started_value")]
    [Precision(10, 4)]
    public decimal? OfflineTrialsStartedValue { get; set; }

    [ForeignKey("LeadgenkpiId")]
    [InverseProperty("Leadgentrial")]
    public virtual LeadgenKpi LeadgenKpi { get; set; } = null!;
}
