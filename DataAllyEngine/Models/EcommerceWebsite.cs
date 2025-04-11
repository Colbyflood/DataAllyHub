using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("ecommercewebsite")]
[Index("EcommercekpiId", Name = "EcommerceWebsite_ECommerce_FK")]
[Index("EcommercekpiId", Name = "ecommercekpi_id_UK", IsUnique = true)]
public partial class EcommerceWebsite
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ecommercekpi_id")]
    public int EcommercekpiId { get; set; }

    [Column("website_add_payment_info")]
    public int? WebsiteAddPaymentInfo { get; set; }

    [Column("website_add_payment_info_value")]
    [Precision(10, 4)]
    public decimal? WebsiteAddPaymentInfoValue { get; set; }

    [Column("cost_per_website_add_payment_info")]
    [Precision(10, 4)]
    public decimal? CostPerWebsiteAddPaymentInfo { get; set; }

    [Column("website_add_to_cart")]
    public int? WebsiteAddToCart { get; set; }

    [Column("website_add_to_cart_value")]
    [Precision(10, 4)]
    public decimal? WebsiteAddToCartValue { get; set; }

    [Column("cost_per_website_add_to_cart")]
    [Precision(10, 4)]
    public decimal? CostPerWebsiteAddToCart { get; set; }

    [Column("website_add_to_wishlist")]
    public int? WebsiteAddToWishlist { get; set; }

    [Column("website_add_to_wishlist_value")]
    [Precision(10, 4)]
    public decimal? WebsiteAddToWishlistValue { get; set; }

    [Column("cost_per_website_add_to_wishlist")]
    [Precision(10, 4)]
    public decimal? CostPerWebsiteAddToWishlist { get; set; }

    [Column("website_checkout_initiated")]
    public int? WebsiteCheckoutInitiated { get; set; }

    [Column("website_checkout_initiated_value")]
    [Precision(10, 4)]
    public decimal? WebsiteCheckoutInitiatedValue { get; set; }

    [Column("cost_per_website_checkout_initiated")]
    [Precision(10, 4)]
    public decimal? CostPerWebsiteCheckoutInitiated { get; set; }

    [Column("website_purchases")]
    public int? WebsitePurchases { get; set; }

    [Column("website_purchases_value")]
    [Precision(10, 4)]
    public decimal? WebsitePurchasesValue { get; set; }

    [Column("cost_per_website_purchases")]
    [Precision(10, 4)]
    public decimal? CostPerWebsitePurchases { get; set; }

    [Column("website_roa")]
    [Precision(10, 4)]
    public decimal? WebsiteRoa { get; set; }

    [ForeignKey("EcommercekpiId")]
    public virtual EcommerceKpi EcommerceKpi { get; set; } = null!;
}
