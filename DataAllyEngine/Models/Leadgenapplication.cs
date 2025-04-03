using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("leadgenapplication")]
[Index("LeadgenkpiId", Name = "LeadGenApplication_LeadGen_FK")]
[Index("LeadgenkpiId", Name = "leadgenkpi_id_UNIQUE", IsUnique = true)]
public partial class Leadgenapplication
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("leadgenkpi_id")]
    public int LeadgenkpiId { get; set; }

    [Column("submit_applications")]
    public int? SubmitApplications { get; set; }

    [Column("submit_applications_value")]
    [Precision(10, 4)]
    public decimal? SubmitApplicationsValue { get; set; }

    [Column("cost_per_submit_applications")]
    [Precision(10, 4)]
    public decimal? CostPerSubmitApplications { get; set; }

    [Column("website_submit_applications")]
    public int? WebsiteSubmitApplications { get; set; }

    [Column("website_submit_applications_value")]
    [Precision(10, 4)]
    public decimal? WebsiteSubmitApplicationsValue { get; set; }

    [Column("cost_per_website_submit_applications")]
    [Precision(10, 4)]
    public decimal? CostPerWebsiteSubmitApplications { get; set; }

    [Column("mobile_app_submit_applications")]
    public int? MobileAppSubmitApplications { get; set; }

    [Column("mobile_app_submit_applications_value")]
    [Precision(10, 4)]
    public decimal? MobileAppSubmitApplicationsValue { get; set; }

    [Column("cost_per_mobile_app_submit_applications")]
    [Precision(10, 4)]
    public decimal? CostPerMobileAppSubmitApplications { get; set; }

    [Column("offline_submit_applications")]
    public int? OfflineSubmitApplications { get; set; }

    [Column("offline_submit_applications_value")]
    [Precision(10, 4)]
    public decimal? OfflineSubmitApplicationsValue { get; set; }

    [Column("cost_per_offline_submit_applications")]
    [Precision(10, 4)]
    public decimal? CostPerOfflineSubmitApplications { get; set; }

    [ForeignKey("LeadgenkpiId")]
    [InverseProperty("Leadgenapplication")]
    public virtual Leadgenkpi Leadgenkpi { get; set; } = null!;
}
