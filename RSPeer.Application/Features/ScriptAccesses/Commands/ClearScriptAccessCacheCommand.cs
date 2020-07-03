using MediatR;

namespace RSPeer.Application.Features.ScriptAccesses.Commands
{
	public class ClearScriptAccessCacheCommand : IRequest<Unit>
	{
		public int UserId { get; set; }
	}
}