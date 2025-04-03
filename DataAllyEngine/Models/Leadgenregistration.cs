using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("leadgenregistration")]
[Index("LeadgenkpiId", Name = "LeadGenRegistration_LeadGen_FK")]
[Index("LeadgenkpiId", Name = "leadgenkpi_id_UNIQUE", IsUnique = true)]
public partial class Leadgenregistration
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("leadgenkpi_id")]
    public int LeadgenkpiId { get; set; }

    [Column("registrations_completed")]
    public int? RegistrationsCompleted { get; set; }

    [Column("registrations_completed_value")]
    [Precision(10, 4)]
    public decimal? RegistrationsCompletedValue { get; set; }

    [Column("cost_per_registrations_completed")]
    [Precision(10, 4)]
    public decimal? CostPerRegistrationsCompleted { get; set; }

    [Column("website_registrations_completed")]
    public int? WebsiteRegistrationsCompleted { get; set; }

    [Column("website_registrations_completed_value")]
    [Precision(10, 4)]
    public decimal? WebsiteRegistrationsCompletedValue { get; set; }

    [Column("cost_per_website_registrations_completed")]
    [Precision(10, 4)]
    public decimal? CostPerWebsiteRegistrationsCompleted { get; set; }

    [Column("mobile_app_registrations_completed")]
    public int? MobileAppRegistrationsCompleted { get; set; }

    [Column("mobile_app_registrations_completed_value")]
    [Precision(10, 4)]
    public decimal? MobileAppRegistrationsCompletedValue { get; set; }

    [ForeignKey("LeadgenkpiId")]
    [InverseProperty("Leadgenregistration")]
    public virtual Leadgenkpi Leadgenkpi { get; set; } = null!;
}
