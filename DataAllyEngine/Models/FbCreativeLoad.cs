using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;


[Table("fbcreativeload")]
[Index("CompanyId", Name = "creativeload_company_fk_idx")]
[Index("ChannelId", Name = "creativeload_channel_fk_idx")]
[Index("ChannelAdId", Name = "creativeload_channel_ad_id_idx")]
[Index("Guid", Name = "creativeload_guid_uk", IsUnique = true)]
public class FbCreativeLoad
{
	[Key]
	[Column("id")]
	public int Id { get; set; }

	[Column("company_id")]
	public int CompanyId { get; set; }
	
	[Column("channel_id")]
	public int ChannelId { get; set; }

	[Column("channel_ad_id")]
	[StringLength(255)]
	public string ChannelAdId { get; set; } = null!;

	[Column("creative_type")]
	[StringLength(15)]
	public string CreativeType { get; set; } = null!;
	
	[Column("creative_key")]
	[StringLength(255)]
	public string CreativeKey { get; set; } = null!;
	
	[Column("creative_page_id")]
	[StringLength(45)]
	public string CreativePageId { get; set; } = null!;
	
	[Column("filename")]
	[StringLength(255)]
	public string? Filename { get; set; }

	[Column("bin_id")]
	public int? BinId { get; set; }

	[Column("guid")]
	public string? Guid { get; set; }

	[Column("extension")]
	[StringLength(10)]
	public string? Extension { get; set; }
	
	[Column("created_datetime_utc", TypeName = "datetime")]
	public DateTime CreatedDateTimeUtc { get; set; }

	[Column("last_attempt_datetime_utc", TypeName = "datetime")]
	public DateTime? LastAttemptDateTimeUtc { get; set; }
	
	[Column("url")]
	[StringLength(4096)]
	public string? Url { get; set; }
	
	[Column("total_attempts")]
	public int TotalAttempts { get; set; }
}