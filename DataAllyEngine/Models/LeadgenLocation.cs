using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("leadgenlocation")]
[Index("LeadgenkpiId", Name = "LeadGenLocation_LeadGen_FK")]
[Index("LeadgenkpiId", Name = "leadgenkpi_id_UNIQUE", IsUnique = true)]
public partial class LeadgenLocation
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("leadgenkpi_id")]
    public int LeadgenkpiId { get; set; }

    [Column("find_locations")]
    public int? FindLocations { get; set; }

    [Column("find_locations_value")]
    [Precision(10, 4)]
    public decimal? FindLocationsValue { get; set; }

    [Column("cost_per_find_locations")]
    [Precision(10, 4)]
    public decimal? CostPerFindLocations { get; set; }

    [Column("website_find_locations")]
    public int? WebsiteFindLocations { get; set; }

    [Column("website_find_locations_value")]
    [Precision(10, 4)]
    public decimal? WebsiteFindLocationsValue { get; set; }

    [Column("cost_per_website_find_locations")]
    [Precision(10, 4)]
    public decimal? CostPerWebsiteFindLocations { get; set; }

    [Column("mobile_app_find_locations")]
    public int? MobileAppFindLocations { get; set; }

    [Column("mobile_app_find_locations_value")]
    [Precision(10, 4)]
    public decimal? MobileAppFindLocationsValue { get; set; }

    [Column("cost_per_mobile_app_find_locations")]
    [Precision(10, 4)]
    public decimal? CostPerMobileAppFindLocations { get; set; }

    [Column("offline_find_locations")]
    public int? OfflineFindLocations { get; set; }

    [Column("offline_find_locations_value")]
    [Precision(10, 4)]
    public decimal? OfflineFindLocationsValue { get; set; }

    [Column("cost_per_offline_find_locations")]
    [Precision(10, 4)]
    public decimal? CostPerOfflineFindLocations { get; set; }

    [ForeignKey("LeadgenkpiId")]
    [InverseProperty("Leadgenlocation")]
    public virtual LeadgenKpi LeadgenKpi { get; set; } = null!;
}
