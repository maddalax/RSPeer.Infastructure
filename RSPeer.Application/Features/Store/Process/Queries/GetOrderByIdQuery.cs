using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Store.Process.Queries
{
	public class GetOrderByIdQuery : IRequest<Order>
	{
		public int OrderId { get; set; }
		public bool IncludeItem { get; set; }	
		public int? UserId { get; set; }
		
		public bool IsAdmin { get; set; }
	}
}