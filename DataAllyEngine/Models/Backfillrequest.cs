using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("backfillrequest")]
[Index("ChannelsourceflowId", Name = "channelsourceflow1_fk_idx")]
[Index("ChannelsourceflowId", Name = "channelsourceflow_id_UNIQUE", IsUnique = true)]
public partial class Backfillrequest
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("channelsourceflow_id")]
    public int ChannelsourceflowId { get; set; }

    [Column("requested_utc", TypeName = "datetime")]
    public DateTime RequestedUtc { get; set; }

    [ForeignKey("ChannelsourceflowId")]
    [InverseProperty("Backfillrequest")]
    public virtual Channelsourceflow Channelsourceflow { get; set; } = null!;
}
