using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("companytype")]
[Index("Name", Name = "companytype_name_UK", IsUnique = true)]
public partial class Companytype
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(45)]
    public string Name { get; set; } = null!;

    [InverseProperty("TypeNavigation")]
    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
}
