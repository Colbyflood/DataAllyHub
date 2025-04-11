using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("asset")]
[Index("ChannelId", "AssetType", "AssetKey", Name = "channel_key_uk", IsUnique = true)]
public partial class Asset
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("channel_id")]
    public int ChannelId { get; set; }

    /// <summary>
    /// values are IMAGE, VIDEO, DPA, CATALOG, CAROUSEL
    /// </summary>
    [Column("asset_type")]
    [StringLength(20)]
    public string AssetType { get; set; } = null!;

    [Column("asset_key")]
    public string AssetKey { get; set; } = null!;

    [Column("thumbnail_guid")]
    [StringLength(50)]
    public string? ThumbnailGuid { get; set; }

    [Column("asset_name")]
    [StringLength(1024)]
    public string? AssetName { get; set; }

    [Column("url")]
    [StringLength(1024)]
    public string? Url { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("updated", TypeName = "datetime")]
    public DateTime? Updated { get; set; }

    [ForeignKey("ChannelId")]
    [InverseProperty("Assets")]
    public virtual Channel Channel { get; set; } = null!;
}