using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("channel")]
[Index("AttributionId", Name = "Channel_Attribution_FK")]
[Index("ChannelTypeId", Name = "Channel_ChannelType_FK")]
[Index("ClientId", "ChannelAccountId", Name = "Channel_ClientId_AccountId_UK", IsUnique = true)]
[Index("ClientId", Name = "Channel_ClientId_FK_idx")]
public partial class Channel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [Column("channel_type_id")]
    public int ChannelTypeId { get; set; }

    [Column("channel_account_id")]
    public string ChannelAccountId { get; set; } = null!;

    [Column("channel_account_name")]
    [StringLength(255)]
    public string? ChannelAccountName { get; set; }

    [Column("channel_token_id")]
    public int? ChannelTokenId { get; set; }

    [Column("channel_auth_token")]
    [StringLength(255)]
    public string? ChannelAuthToken { get; set; }

    [Column("channel_timezone")]
    [StringLength(50)]
    public string? ChannelTimezone { get; set; }

    [Column("channel_timezone_utc_bias")]
    public int? ChannelTimezoneUtcBias { get; set; }

    [Column("channel_currency")]
    [StringLength(10)]
    public string ChannelCurrency { get; set; } = null!;

    [Column("channel_status")]
    [StringLength(255)]
    public string? ChannelStatus { get; set; }

    [Column("attribution_id")]
    public int? AttributionId { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("updated", TypeName = "datetime")]
    public DateTime? Updated { get; set; }

    [InverseProperty("Channel")]
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
    
    [ForeignKey("AttributionId")]
    [InverseProperty("Channels")]
    public virtual Attribution? Attribution { get; set; }

    [InverseProperty("Channel")]
    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

    [ForeignKey("ChannelTypeId")]
    [InverseProperty("Channels")]
    public virtual ChannelType ChannelType { get; set; } = null!;

    [InverseProperty("Channel")]
    public virtual ICollection<Channelsourceflow> Channelsourceflows { get; set; } = new List<Channelsourceflow>();

    [ForeignKey("ClientId")]
    [InverseProperty("Channels")]
    public virtual Client Client { get; set; } = null!;

    [InverseProperty("Channel")]
    public virtual ICollection<FbBackfillRequest> Fbbackfillrequests { get; set; } = new List<FbBackfillRequest>();
    
    [InverseProperty("Channel")]
    public virtual FbDailySchedule? Fbdailyschedule { get; set; }

    [InverseProperty("Channel")]
    public virtual ICollection<FbRunLog> Fbrunlogs { get; set; } = new List<FbRunLog>();
}
