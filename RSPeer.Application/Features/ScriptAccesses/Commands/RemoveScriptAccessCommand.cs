using MediatR;

namespace RSPeer.Application.Features.ScriptAccesses.Commands
{
	public class RemoveScriptAccessCommand : IRequest<Unit>
	{
		public int UserId { get; set; }
		public int ScriptId { get; set; }
	}
}