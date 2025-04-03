using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("account")]
[Index("AccountTypeId", Name = "Acct_AcctType_FK")]
[Index("CompanyId", Name = "Acct_Company_FK_idx")]
public partial class Account
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("company_id")]
    public int CompanyId { get; set; }

    [Column("account_type_id")]
    public int AccountTypeId { get; set; }

    [Column("active", TypeName = "bit(1)")]
    public ulong Active { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("updated", TypeName = "datetime")]
    public DateTime? Updated { get; set; }

    [ForeignKey("AccountTypeId")]
    [InverseProperty("Accounts")]
    public virtual Accounttype AccountType { get; set; } = null!;

    [InverseProperty("Account")]
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

    [ForeignKey("CompanyId")]
    [InverseProperty("Accounts")]
    public virtual Company Company { get; set; } = null!;
}
