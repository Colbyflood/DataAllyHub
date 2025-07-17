using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("fbaccountpagetoken")]
[Index("TokenId", Name = "fbaccounttoken_ibfk_1")]
[Index("TokenId", Name = "token_id")]
[Index("FbAccountId", Name = "fb_account_id")]
public class FbAccountPageToken
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("fb_account_name")]
    public string? FbAccountName { get; set; }

    [Column("fb_account_id")]
    public string? FbAccountId { get; set; }

    [Column("token_id")]
    public int? TokenId { get; set; }

    [Column("access_token")]
    [StringLength(255)]
    public string? PageAccessToken { get; set; }

    [Column("created_utc", TypeName = "datetime")]
    public DateTime? CreatedDateTimeUtc { get; set; }

    [Column("updated_utc", TypeName = "datetime")]
    public DateTime? UpdatedDateTime { get; set; }

    [ForeignKey("TokenId")]
    public virtual Token? Token { get; set; } = null;

}