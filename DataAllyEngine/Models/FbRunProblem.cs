using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("fbrunproblem")]
[Index("FbRunlogId", Name = "fbrunproblem_fbrunlog_fk_idx")]
public partial class FbRunProblem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("fb_runlog_id")]
    public int FbRunlogId { get; set; }

    [Column("reason")]
    [StringLength(2048)]
    public string Reason { get; set; } = null!;

    [Column("created_utc", TypeName = "datetime")]
    public DateTime CreatedUtc { get; set; }

    [Column("restart_after_utc", TypeName = "datetime")]
    public DateTime? RestartAfterUtc { get; set; }

    [Column("restart_url")]
    [StringLength(2048)]
    public string? RestartUrl { get; set; }

    [Column("restarted_utc", TypeName = "datetime")]
    public DateTime? RestartedUtc { get; set; }

    [Column("fb_api_response")]
    public string? FbErrorResponse { get; set; }

    [ForeignKey("FbRunlogId")]
    [InverseProperty("Fbrunproblems")]
    public virtual FbRunLog FbRunlog { get; set; } = null!;
}
