using MediatR;

namespace RSPeer.Application.Features.Store.Process.Commands
{
	public class RefundOrderCommand : IRequest<Unit>
	{
		public int OrderId { get; set; }
		public int AdminUserId { get; set; }
	}
}