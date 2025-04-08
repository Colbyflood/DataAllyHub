using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("channeltype")]
[Index("Name", Name = "ChannelType_Name_UQ", IsUnique = true)]
public partial class ChannelType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [InverseProperty("ChannelType")]
    public virtual ICollection<Channel> Channels { get; set; } = new List<Channel>();

    [InverseProperty("ChannelType")]
    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
}
