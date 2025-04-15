using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("fbbackfillrequest")]
[Index("ChannelId", Name = "fbbackfillrequest_channel_fk_idx")]
public partial class FbBackfillRequest
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("channel_id")]
    public int ChannelId { get; set; }

    [Column("days")]
    public int Days { get; set; }

    [Column("requested_utc", TypeName = "datetime")]
    public DateTime RequestedUtc { get; set; }

    [ForeignKey("ChannelId")]
    public virtual Channel Channel { get; set; } = null!;
}
