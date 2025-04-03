using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("leadgencontact")]
[Index("LeadgenkpiId", Name = "LeadGenContact_LeadGen_FK")]
[Index("LeadgenkpiId", Name = "leadgenkpi_id_UNIQUE", IsUnique = true)]
public partial class Leadgencontact
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("leadgenkpi_id")]
    public int LeadgenkpiId { get; set; }

    [Column("contacts")]
    public int? Contacts { get; set; }

    [Column("contacts_value")]
    [Precision(10, 4)]
    public decimal? ContactsValue { get; set; }

    [Column("cost_per_contacts")]
    [Precision(10, 4)]
    public decimal? CostPerContacts { get; set; }

    [Column("website_contacts")]
    public int? WebsiteContacts { get; set; }

    [Column("website_contacts_value")]
    [Precision(10, 4)]
    public decimal? WebsiteContactsValue { get; set; }

    [Column("cost_per_website_contacts")]
    [Precision(10, 4)]
    public decimal? CostPerWebsiteContacts { get; set; }

    [Column("mobile_app_contacts")]
    public int? MobileAppContacts { get; set; }

    [Column("mobile_app_contacts_value")]
    [Precision(10, 4)]
    public decimal? MobileAppContactsValue { get; set; }

    [Column("cost_per_mobile_app_contacts")]
    [Precision(10, 4)]
    public decimal? CostPerMobileAppContacts { get; set; }

    [Column("offline_contacts")]
    public int? OfflineContacts { get; set; }

    [Column("offline_contacts_value")]
    [Precision(10, 4)]
    public decimal? OfflineContactsValue { get; set; }

    [Column("cost_per_offline_contacts")]
    [Precision(10, 4)]
    public decimal? CostPerOfflineContacts { get; set; }

    [ForeignKey("LeadgenkpiId")]
    [InverseProperty("Leadgencontact")]
    public virtual Leadgenkpi Leadgenkpi { get; set; } = null!;
}
