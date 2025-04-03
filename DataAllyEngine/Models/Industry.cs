using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("industry")]
[Index("Name", Name = "Industry_Name_UQ", IsUnique = true)]
public partial class Industry
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [InverseProperty("Industry")]
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
}
