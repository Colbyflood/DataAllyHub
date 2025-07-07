using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("fbsavecontent")]
[Index("AdCreativeRunlogId", Name = "fbsave_ad_creative_fb_idx")]
[Index("AdImageRunlogId", Name = "fbsave_ad_image_fk_idx")]
[Index("AdInsightRunlogId", Name = "fbsave_ad_insight_fk_idx")]
public partial class FbSaveContent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ad_creative_runlog_id")]
    public int? AdCreativeRunlogId { get; set; }

    [Column("ad_image_runlog_id")]
    public int? AdImageRunlogId { get; set; }

    [Column("ad_insight_runlog_id")]
    public int? AdInsightRunlogId { get; set; }

    [Column("queued_utc", TypeName = "datetime")]
    public DateTime? QueuedUtc { get; set; }

    [Column("started_utc", TypeName = "datetime")]
    public DateTime? StartedUtc { get; set; }  // TODO: This might be created datetime

    [Column("last_started_utc", TypeName = "datetime")]
    public DateTime? LastStartedUtc { get; set; }

    [Column("ad_creative_finished_utc", TypeName = "datetime")]
    public DateTime? AdCreativeFinishedUtc { get; set; }

    [Column("ad_image_finished_utc", TypeName = "datetime")]
    public DateTime? AdImageFinishedUtc { get; set; }

    [Column("ad_insight_finished_utc", TypeName = "datetime")]
    public DateTime? AdInsightFinishedUtc { get; set; }

    [Column("attempts")]
    public int Attempts { get; set; }

    [Column("sequence")]
    public int Sequence { get; set; }

    [Column("heart_beat_last_received_at_utc", TypeName = "datetime")]
    public DateTime? HeartBeatLastReceivedAtUtc { get; set; }

    [Column("error")]
    public string? ErrorMessage { get; set; }

    [ForeignKey("AdCreativeRunlogId")]
    [InverseProperty("FbsavecontentAdCreativeRunlogs")]
    public virtual FbRunLog AdCreativeRunlog { get; set; } = null!;

    [ForeignKey("AdImageRunlogId")]
    [InverseProperty("FbsavecontentAdImageRunlogs")]
    public virtual FbRunLog AdImageRunlog { get; set; } = null!;

    [ForeignKey("AdInsightRunlogId")]
    [InverseProperty("FbsavecontentAdInsightRunlogs")]
    public virtual FbRunLog AdInsightRunlog { get; set; } = null!;
}
