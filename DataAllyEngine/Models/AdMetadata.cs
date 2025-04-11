using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("admetadata")]
[Index("AdId", Name = "AdMetadata_Ad_FK")]
[Index("AdId", Name = "ad_adid_UK", IsUnique = true)]
public partial class AdMetadata
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ad_id")]
    public int AdId { get; set; }

    [Column("effective_date", TypeName = "datetime")]
    public DateTime? EffectiveDate { get; set; }

    [Column("status")]
    [StringLength(255)]
    public string? Status { get; set; }

    [Column("destination_url")]
    [StringLength(1024)]
    public string? DestinationUrl { get; set; }

    [Column("tracking_template_url")]
    [StringLength(1024)]
    public string? TrackingTemplateUrl { get; set; }

    [Column("utm")]
    [StringLength(1024)]
    public string? Utm { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("updated", TypeName = "datetime")]
    public DateTime? Updated { get; set; }

    [ForeignKey("AdId")]
    [InverseProperty("AdMetadata")]
    public virtual Ad Ad { get; set; } = null!;
}
