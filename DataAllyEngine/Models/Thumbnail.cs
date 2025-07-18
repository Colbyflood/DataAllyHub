using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("thumbnail")]
[Index("ChannelAdId", Name = "channel_ad_id_IDX")]
[Index("Guid", Name = "guid_UK", IsUnique = true)]
[Index("ChannelId", Name = "thumbnail_channel_fk")]
public partial class Thumbnail
{
	[Key]
	[Column("id")]
	public int Id { get; set; }

	[Column("filename")]
	[StringLength(255)]
	public string Filename { get; set; } = null!;

	[Column("channel_ad_id")]
	public string ChannelAdId { get; set; } = null!;

	[Column("bin_id")]
	public int BinId { get; set; }

	[Column("guid")]
	public string Guid { get; set; } = null!;

	[Column("extension")]
	[StringLength(10)]
	public string Extension { get; set; } = null!;

    [Column("channel_id")]
    public int? ChannelId { get; set; }
}