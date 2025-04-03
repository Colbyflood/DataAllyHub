using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("attribution")]
[Index("Name", Name = "Attribution_Name_UQ", IsUnique = true)]
public partial class Attribution
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [InverseProperty("Attribution")]
    public virtual ICollection<Channel> Channels { get; set; } = new List<Channel>();
}
