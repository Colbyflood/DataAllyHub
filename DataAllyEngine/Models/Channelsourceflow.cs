using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("channelsourceflow")]
[Index("ChannelId", Name = "account_filesequence_fk_idx")]
[Index("ChannelId", "FileSequence", "Purpose", Name = "channel_sequence_purpose_uk", IsUnique = true)]
public partial class Channelsourceflow
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("channel_id")]
    public int ChannelId { get; set; }

    [Column("static_id")]
    [StringLength(50)]
    public string StaticId { get; set; } = null!;

    [Column("file_sequence")]
    public int FileSequence { get; set; }

    [Column("source_id")]
    [StringLength(50)]
    public string SourceId { get; set; } = null!;

    [Column("flow_id")]
    [StringLength(50)]
    public string FlowId { get; set; } = null!;

    [Column("purpose")]
    [StringLength(20)]
    public string Purpose { get; set; } = null!;

    [Column("created_utc", TypeName = "datetime")]
    public DateTime CreatedUtc { get; set; }

    [InverseProperty("Channelsourceflow")]
    public virtual BackfillFlowRequest? Backfillflowrequest { get; set; }

    [InverseProperty("Channelsourceflow")]
    public virtual BackfillRequest? Backfillrequest { get; set; }

    [ForeignKey("ChannelId")]
    [InverseProperty("Channelsourceflows")]
    public virtual Channel Channel { get; set; } = null!;
}
