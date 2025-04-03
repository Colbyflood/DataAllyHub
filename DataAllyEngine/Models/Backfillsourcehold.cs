using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("backfillsourcehold")]
[Index("ChannelId", Name = "channel_id_UNIQUE", IsUnique = true)]
public partial class Backfillsourcehold
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("channel_id")]
    public int ChannelId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("expiration_utc", TypeName = "datetime")]
    public DateTime ExpirationUtc { get; set; }
}
