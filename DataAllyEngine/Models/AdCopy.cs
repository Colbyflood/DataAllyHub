using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("adcopy")]
[Index("AdId", Name = "adcopy_ad_fk_idx")]
[Index("AdId", "Hash", Name = "adcopy_hash_uk", IsUnique = true)]
public partial class AdCopy
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ad_id")]
    public int AdId { get; set; }

    [Column("hash")]
    [StringLength(256)]
    public string Hash { get; set; } = null!;

    [Column("title")]
    [StringLength(1024)]
    public string Title { get; set; } = null!;

    [Column("copy", TypeName = "mediumtext")]
    public string Copy { get; set; } = null!;

    [ForeignKey("AdId")]
    [InverseProperty("Adcopies")]
    public virtual Ad Ad { get; set; } = null!;
}
