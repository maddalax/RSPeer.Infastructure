namespace RSPeer.Domain.Entities
{
	public class PricePerQuantity
    {
		public int Id { get; set; }
		public string Sku { get; set; }
		public int Quantity { get; set; }
		public int Price { get; set; }
    }
}