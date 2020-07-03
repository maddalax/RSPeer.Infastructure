using System;
using RSPeer.Domain.Entities;

namespace RSPeer.Domain.Dtos
{
	public class ItemDto
	{
		public ItemDto(Item item)
		{
			Id = item.Id;
			Name = item.Name;
			Description = item.Description;
			Sku = item.Sku;
			Price = item.Price;
			PaymentMethod = item.PaymentMethod;
			ExpirationInMinutes = item.ExpirationInMinutes;
			if (item.ExpirationInMinutes.HasValue)
			{
				ExpirationDays = TimeSpan.FromMinutes(item.ExpirationInMinutes.Value).TotalDays;
			}

			Type = item.Type;
		}
		
		public int Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }
		
		public string Sku { get; set; }
		
		public decimal Price { get; set; }
				
		public PaymentMethod PaymentMethod { get; set; }
		
		public int? ExpirationInMinutes { get; set; }
		
		public double? ExpirationDays { get; set; }

		public ItemType Type { get; set; }
		
	}
}