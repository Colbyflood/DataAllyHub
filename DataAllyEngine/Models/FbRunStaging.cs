using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("fbrunstaging")]
[Index("FbRunlogId", Name = "fbstaging_runlog_fk_idx")]
[Index("FbRunlogId", "Sequence", Name = "fbstaging_runlog_uk", IsUnique = true)]
public partial class FbRunStaging
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("fb_runlog_id")]
    public int FbRunlogId { get; set; }

    [Column("sequence")]
    public int Sequence { get; set; }

    [Column("content")]
    public string Content { get; set; }

    [Column("rows_count")]
    public int? RowsCount { get; set; }

    [ForeignKey("FbRunlogId")]
    public virtual FbRunLog FbRunlog { get; set; } = null!;
}
