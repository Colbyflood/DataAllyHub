using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("tokenfbaccount")]
[Index("TokenId", "AccountNumber", Name = "TokenIdAccountNumberUK", IsUnique = true)]
public partial class TokenFbAccount
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("token_id")]
    public int TokenId { get; set; }

    [Column("account_number")]
    [StringLength(45)]
    public string AccountNumber { get; set; } = null!;

    [Column("account_id")]
    [StringLength(45)]
    public string AccountId { get; set; } = null!;

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("business_name")]
    [StringLength(255)]
    public string BusinessName { get; set; } = null!;

    [Column("currency")]
    [StringLength(3)]
    public string Currency { get; set; } = null!;

    [Column("timezone_name")]
    [StringLength(100)]
    public string TimezoneName { get; set; } = null!;

    [Column("timezone_offset")]
    public int TimezoneOffset { get; set; }

    [Column("loaded_date_utc", TypeName = "datetime")]
    public DateTime LoadedDateUtc { get; set; }

    [ForeignKey("TokenId")]
    [InverseProperty("Tokenfbaccounts")]
    public virtual Token Token { get; set; } = null!;
}
