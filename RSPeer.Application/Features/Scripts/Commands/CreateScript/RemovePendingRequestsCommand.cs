using MediatR;

namespace RSPeer.Application.Features.Scripts.Commands.CreateScript
{
	public class RemovePendingRequestsCommand : IRequest<Unit>
	{
		public int ScriptId { get; set; }
	}
}