using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("fbrunlog")]
[Index("ChannelId", Name = "fbrunlog_channel_fk_idx")]
public partial class FbRunLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("channel_id")]
    public int ChannelId { get; set; }

    [Column("feed_type")]
    [StringLength(45)]
    public string FeedType { get; set; } = null!;

    [Column("scope_type")]
    [StringLength(45)]
    public string ScopeType { get; set; } = null!;

    [Column("backfill_days")]
    public int? BackfillDays { get; set; }

    [Column("started_utc", TypeName = "datetime")]
    public DateTime StartedUtc { get; set; }

    [Column("last_started_utc", TypeName = "datetime")]
    public DateTime LastStartedUtc { get; set; }
    
    [Column("finished_utc", TypeName = "datetime")]
    public DateTime? FinishedUtc { get; set; }

    [ForeignKey("ChannelId")]
    [InverseProperty("Fbrunlogs")]
    public virtual Channel Channel { get; set; } = null!;

    [InverseProperty("FbRunlog")]
    public virtual ICollection<FbRunProblem> Fbrunproblems { get; set; } = new List<FbRunProblem>();

    [InverseProperty("FbRunlog")]
    public virtual ICollection<FbRunStaging> Fbrunstagings { get; set; } = new List<FbRunStaging>();

    [InverseProperty("AdCreativeRunlog")]
    public virtual ICollection<FbSaveContent> FbsavecontentAdCreativeRunlogs { get; set; } = new List<FbSaveContent>();

    [InverseProperty("AdImageRunlog")]
    public virtual ICollection<FbSaveContent> FbsavecontentAdImageRunlogs { get; set; } = new List<FbSaveContent>();

    [InverseProperty("AdInsightRunlog")]
    public virtual ICollection<FbSaveContent> FbsavecontentAdInsightRunlogs { get; set; } = new List<FbSaveContent>();
}
