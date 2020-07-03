using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;

namespace RSPeer.Application.Features.Store.Paypal.Models
{
	public class PaypalOrder
	{
		[JsonPropertyName("intent")] public string Intent { get; set; }

		[JsonPropertyName("payer")] public Payer Payer { get; set; }

		[JsonPropertyName("transactions")] public List<Transaction> Transactions { get; set; }

		[JsonPropertyName("note_to_payer")] public string NoteToPayer { get; set; }

		[JsonPropertyName("redirect_urls")] public RedirectUrls RedirectUrls { get; set; }
	}

	public class Payer
	{
		[JsonPropertyName("payment_method")] public string PaymentMethod { get; set; }
	}

	public class RedirectUrls
	{
		[JsonPropertyName("return_url")] public string ReturnUrl { get; set; }

		[JsonPropertyName("cancel_url")] public string CancelUrl { get; set; }
	}

	public class Transaction
	{
		[JsonPropertyName("amount")] public Amount Amount { get; set; }

		[JsonPropertyName("description")] public string Description { get; set; }

		[JsonPropertyName("custom")] public string Custom { get; set; }

		[JsonPropertyName("invoice_number")] public string InvoiceNumber { get; set; }

		[JsonPropertyName("payment_options")] public PaymentOptions PaymentOptions { get; set; }

		[JsonPropertyName("soft_descriptor")] public string SoftDescriptor { get; set; }

		[JsonPropertyName("item_list")] public ItemList ItemList { get; set; }
	}

	public class Amount
	{
		[JsonPropertyName("total")] public string Total { get; set; }

		[JsonPropertyName("currency")] public string Currency { get; set; }

		[JsonPropertyName("details")] public Details Details { get; set; }
	}

	public class Details
	{
		[JsonPropertyName("subtotal")] public string Subtotal { get; set; }

		[JsonPropertyName("tax")] public string Tax { get; set; }

		[JsonPropertyName("shipping")] public string Shipping { get; set; }

		[JsonPropertyName("handling_fee")] public string HandlingFee { get; set; }

		[JsonPropertyName("shipping_discount")] public string ShippingDiscount { get; set; }

		[JsonPropertyName("insurance")] public string Insurance { get; set; }
	}

	public class ItemList
	{
		[JsonPropertyName("items")] public List<Item> Items { get; set; }

		[JsonPropertyName("shipping_address")] public ShippingAddress ShippingAddress { get; set; }
	}

	public class Item
	{
		[JsonPropertyName("name")] public string Name { get; set; }

		[JsonPropertyName("description")] public string Description { get; set; }

		[JsonPropertyName("quantity")] public int Quantity { get; set; }

		[JsonPropertyName("price")] public string Price { get; set; }

		[JsonPropertyName("tax")] public string Tax { get; set; }

		[JsonPropertyName("sku")] public string Sku { get; set; }

		[JsonPropertyName("currency")] public string Currency { get; set; }
	}

	public class ShippingAddress
	{
		[JsonPropertyName("recipient_name")] public string RecipientName { get; set; }

		[JsonPropertyName("line1")] public string Line1 { get; set; }

		[JsonPropertyName("line2")] public string Line2 { get; set; }

		[JsonPropertyName("city")] public string City { get; set; }

		[JsonPropertyName("country_code")] public string CountryCode { get; set; }

		[JsonPropertyName("postal_code")] public string PostalCode { get; set; }

		[JsonPropertyName("phone")] public string Phone { get; set; }

		[JsonPropertyName("state")] public string State { get; set; }
	}

	public class PaymentOptions
	{
		[JsonPropertyName("allowed_payment_method")]
		public string AllowedPaymentMethod { get; set; }
	}
}