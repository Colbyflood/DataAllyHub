using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("token")]
[Index("CompanyId", "ChannelTypeId", Name = "company_channel_uk", IsUnique = true)]
[Index("ChannelTypeId", Name = "token_channel_type_fk_idx")]
[Index("CompanyId", Name = "token_company_fk_idx")]
public partial class Token
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("company_id")]
    public int CompanyId { get; set; }

    [Column("channel_type_id")]
    public int ChannelTypeId { get; set; }

    [Column("renewed_utc")]
    [MaxLength(6)]
    public DateTime RenewedUtc { get; set; }

    [Column("token")]
    [StringLength(1024)]
    public string Token1 { get; set; } = null!;

    [Column("enabled", TypeName = "bit(1)")]
    public ulong Enabled { get; set; }

    [ForeignKey("ChannelTypeId")]
    [InverseProperty("Tokens")]
    public virtual ChannelType ChannelType { get; set; } = null!;

    [ForeignKey("CompanyId")]
    [InverseProperty("Tokens")]
    public virtual Company Company { get; set; } = null!;

    [InverseProperty("Token")]
    public virtual ICollection<TokenFbAccount> Tokenfbaccounts { get; set; } = new List<TokenFbAccount>();
}
