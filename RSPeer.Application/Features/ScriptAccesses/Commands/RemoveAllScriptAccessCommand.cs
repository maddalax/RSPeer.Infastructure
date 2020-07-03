using MediatR;

namespace RSPeer.Application.Features.ScriptAccesses.Commands
{
	public class RemoveAllScriptAccessCommand : IRequest<Unit>
	{
		public int ScriptId { get; set; }
	}
}