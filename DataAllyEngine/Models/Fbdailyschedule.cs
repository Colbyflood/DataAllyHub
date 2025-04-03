using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("fbdailyschedule")]
[Index("ChannelId", Name = "channel_id_UNIQUE", IsUnique = true)]
public partial class Fbdailyschedule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("channel_id")]
    public int ChannelId { get; set; }

    [Column("trigger_hour_utc")]
    public int TriggerHourUtc { get; set; }

    [Column("created_utc", TypeName = "datetime")]
    public DateTime CreatedUtc { get; set; }

    [Column("last_started_utc", TypeName = "datetime")]
    public DateTime? LastStartedUtc { get; set; }

    [Column("last_finished_utc", TypeName = "datetime")]
    public DateTime? LastFinishedUtc { get; set; }

    [ForeignKey("ChannelId")]
    [InverseProperty("Fbdailyschedule")]
    public virtual Channel Channel { get; set; } = null!;
}
