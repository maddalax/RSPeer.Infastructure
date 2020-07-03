using System.Collections.Generic;
using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Store.Process.Queries
{
	public class GetOrdersQuery : IRequest<IEnumerable<Order>>
	{
		public int? ItemId { get; set; }
		public int? UserId { get; set; }
		
		public string Sku { get; set; }
		public bool IncludeItem { get; set; }
		
		public OrderStatus? Status { get; set; }
		
		public bool? NotExpired { get; set; }
	}
}