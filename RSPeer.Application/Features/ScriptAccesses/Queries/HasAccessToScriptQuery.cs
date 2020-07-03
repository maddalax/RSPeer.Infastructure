using MediatR;

namespace RSPeer.Application.Features.ScriptAccesses.Queries
{
	public class HasAccessToScriptQuery : IRequest<bool>
	{
		public int? UserId { get; set; }
		public int ScriptId { get; set; }
	}
}