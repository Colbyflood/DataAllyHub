using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("ecommercechannel")]
[Index("EcommercekpiId", Name = "EcommerceChannel_ECommerce_FK")]
[Index("EcommercekpiId", Name = "ecommercekpi_id_UK", IsUnique = true)]
public partial class Ecommercechannel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ecommercekpi_id")]
    public int EcommercekpiId { get; set; }

    [Column("channel_add_to_cart")]
    public int? ChannelAddToCart { get; set; }

    [Column("channel_add_to_cart_value")]
    [Precision(10, 4)]
    public decimal? ChannelAddToCartValue { get; set; }

    [Column("channel_cost_per_add_to_cart")]
    [Precision(10, 4)]
    public decimal? ChannelCostPerAddToCart { get; set; }

    [Column("channel_add_to_wishlist")]
    public int? ChannelAddToWishlist { get; set; }

    [Column("channel_add_to_wishlist_value")]
    [Precision(10, 4)]
    public decimal? ChannelAddToWishlistValue { get; set; }

    [Column("channel_checkout_initiated")]
    public int? ChannelCheckoutInitiated { get; set; }

    [Column("channel_checkout_initiated_value")]
    [Precision(10, 4)]
    public decimal? ChannelCheckoutInitiatedValue { get; set; }

    [Column("channel_purchases")]
    public int? ChannelPurchases { get; set; }

    [Column("channel_purchases_value")]
    [Precision(10, 4)]
    public decimal? ChannelPurchasesValue { get; set; }

    [Column("cost_per_channel_purchases")]
    [Precision(10, 4)]
    public decimal? CostPerChannelPurchases { get; set; }

    [Column("channel_roa")]
    [Precision(10, 4)]
    public decimal? ChannelRoa { get; set; }

    [ForeignKey("EcommercekpiId")]
    [InverseProperty("Ecommercechannel")]
    public virtual Ecommercekpi Ecommercekpi { get; set; } = null!;
}
