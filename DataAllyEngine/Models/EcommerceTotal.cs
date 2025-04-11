using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("ecommercetotal")]
[Index("EcommercekpiId", Name = "EcommerceTotal_ECommerce_FK")]
[Index("EcommercekpiId", Name = "ecommercekpi_id_UK", IsUnique = true)]
public partial class EcommerceTotal
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ecommercekpi_id")]
    public int EcommercekpiId { get; set; }

    [Column("total_add_payment_info")]
    public int? TotalAddPaymentInfo { get; set; }

    [Column("total_add_payment_info_value")]
    [Precision(10, 4)]
    public decimal? TotalAddPaymentInfoValue { get; set; }

    [Column("cost_per_total_add_payment_info")]
    [Precision(10, 4)]
    public decimal? CostPerTotalAddPaymentInfo { get; set; }

    [Column("total_add_to_cart")]
    public int? TotalAddToCart { get; set; }

    [Column("total_add_to_cart_value")]
    [Precision(10, 4)]
    public decimal? TotalAddToCartValue { get; set; }

    [Column("cost_per_total_add_to_cart")]
    [Precision(10, 4)]
    public decimal? CostPerTotalAddToCart { get; set; }

    [Column("total_add_to_wishlist")]
    public int? TotalAddToWishlist { get; set; }

    [Column("total_add_to_wishlist_value")]
    [Precision(10, 4)]
    public decimal? TotalAddToWishlistValue { get; set; }

    [Column("cost_per_total_add_to_wishlist")]
    [Precision(10, 4)]
    public decimal? CostPerTotalAddToWishlist { get; set; }

    [Column("total_checkout_initiated")]
    public int? TotalCheckoutInitiated { get; set; }

    [Column("total_checkout_initiated_value")]
    [Precision(10, 4)]
    public decimal? TotalCheckoutInitiatedValue { get; set; }

    [Column("cost_per_total_checkout_initiated")]
    [Precision(10, 4)]
    public decimal? CostPerTotalCheckoutInitiated { get; set; }

    [Column("total_purchases")]
    public int? TotalPurchases { get; set; }

    [Column("total_purchases_value")]
    [Precision(10, 4)]
    public decimal? TotalPurchasesValue { get; set; }

    [Column("total_roa")]
    [Precision(10, 4)]
    public decimal? TotalRoa { get; set; }

    [ForeignKey("EcommercekpiId")]
    public virtual EcommerceKpi EcommerceKpi { get; set; } = null!;
}
