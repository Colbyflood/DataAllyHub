using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("ecommercemobile")]
[Index("EcommercekpiId", Name = "EcommerceMobile_ECommerce_FK")]
[Index("EcommercekpiId", Name = "ecommercekpi_id_UK", IsUnique = true)]
public partial class EcommerceMobile
{
	[Key]
	[Column("id")]
	public int Id { get; set; }

	[Column("ecommercekpi_id")]
	public int EcommercekpiId { get; set; }

	[Column("mobile_app_add_payment_info")]
	[Precision(10, 4)]
	public decimal? MobileAppAddPaymentInfo { get; set; }

	[Column("mobile_app_add_payment_info_value")]
	public int? MobileAppAddPaymentInfoValue { get; set; }

	[Column("mobile_app_add_to_cart")]
	public int? MobileAppAddToCart { get; set; }

	[Column("mobile_app_add_to_cart_value")]
	[Precision(10, 4)]
	public decimal? MobileAppAddToCartValue { get; set; }

	[Column("mobile_app_add_to_wishlist")]
	public int? MobileAppAddToWishlist { get; set; }

	[Column("mobile_app_add_to_wishlist_value")]
	[Precision(10, 4)]
	public decimal? MobileAppAddToWishlistValue { get; set; }

	[Column("mobile_app_checkout_initiated")]
	public int? MobileAppCheckoutInitiated { get; set; }

	[Column("mobile_app_checkout_initiated_value")]
	[Precision(10, 4)]
	public decimal? MobileAppCheckoutInitiatedValue { get; set; }

	[ForeignKey("EcommercekpiId")]
	[InverseProperty("Ecommercemobile")]
	public virtual EcommerceKpi EcommerceKpi { get; set; } = null!;
}
