using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("company")]
[Index("Type", Name = "companytype_id_FK_idx")]
[Index("Id", Name = "id_UNIQUE", IsUnique = true)]
public partial class Company
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("type")]
    public int? Type { get; set; }

    [Column("ownerJobTitle")]
    [StringLength(45)]
    public string? OwnerJobTitle { get; set; }

    [Column("usageGoal")]
    [StringLength(45)]
    public string? UsageGoal { get; set; }

    [InverseProperty("Company")]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    [InverseProperty("Company")]
    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();

    [ForeignKey("Type")]
    [InverseProperty("Companies")]
    public virtual CompanyType? TypeNavigation { get; set; }
}
