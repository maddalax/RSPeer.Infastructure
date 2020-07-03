using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RSPeer.Domain.Entities
{
	public class Item
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }
		public string Sku { get; set; }
		public decimal Price { get; set; }
		
		public decimal? FeesPercent { get; set; }
		
		public PaymentMethod PaymentMethod { get; set; }

		[JsonIgnore]
		[NotMapped]
		public IEnumerable<Order> Orders { get; set; }
		
		public int? ExpirationInMinutes { get; set; }

		public ItemType Type
		{
			get
			{
				if (Sku.StartsWith("premium-script-"))
				{
					return ItemType.PremiumScript;
				}
				
				if (Sku == "tokens")
				{
					return ItemType.Tokens;
				}

				if (Sku == "instances")
				{
					return ItemType.ClientInstances;
				}

				if (Sku == "unlimitedInstances")
				{
					return ItemType.ClientInstances;
				}
				
				return ItemType.Other;
			}
		}
	}

	public enum ItemType
	{
		Tokens,
		ClientInstances,
		PremiumScript,
		Other
	}
	
	public enum PaymentMethod
	{
		Tokens,
		Paypal
	}
}