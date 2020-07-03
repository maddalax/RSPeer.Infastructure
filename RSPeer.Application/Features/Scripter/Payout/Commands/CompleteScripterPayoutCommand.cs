using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Scripter.Payout.Commands
{
	public class CompleteScripterPayoutCommand : IRequest<Unit>
	{
		public ScripterPayout Payout { get; set; }
		public User Admin { get; set; }
	}
}