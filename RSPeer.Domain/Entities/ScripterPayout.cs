using System.Collections.Generic;

namespace RSPeer.Domain.Entities
{
	public class ScripterPayout
	{
		public IEnumerable<Order> Orders { get; set; }
		public User Scripter { get; set; }
		public IEnumerable<Script> Scripts { get; set; }
		public IEnumerable<Item> Items { get; set; }
		
		public decimal TotalSales { get; set; }		
	
		public decimal AmountToPayout { get; set; }
		
		public int RefundedOrderCount { get; set; }
		
		public decimal RefundedOrderTotal { get; set; }
		
		public decimal StaffPurchases { get; set; }
	
		public decimal StaffPurchasesTotal { get; set; }
		
		public int TokensToRemove { get; set; }
	}
}