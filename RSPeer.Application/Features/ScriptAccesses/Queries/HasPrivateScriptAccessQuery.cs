using MediatR;

namespace RSPeer.Application.Features.ScriptAccesses.Queries
{
	public class HasPrivateScriptAccessQuery : IRequest<bool>
	{
		public int UserId { get; set; }
		public int ScriptId { get; set; }
	}
}