using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("client")]
[Index("AccountId", Name = "Client_Account_FK")]
[Index("IndustryId", Name = "Client_Industry_FK")]
public partial class Client
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("account_id")]
    public int AccountId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("industry_id")]
    public int IndustryId { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("updated", TypeName = "datetime")]
    public DateTime? Updated { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("Clients")]
    public virtual Account Account { get; set; } = null!;

    [InverseProperty("Client")]
    public virtual ICollection<Channel> Channels { get; set; } = new List<Channel>();

    [ForeignKey("IndustryId")]
    [InverseProperty("Clients")]
    public virtual Industry Industry { get; set; } = null!;
}
