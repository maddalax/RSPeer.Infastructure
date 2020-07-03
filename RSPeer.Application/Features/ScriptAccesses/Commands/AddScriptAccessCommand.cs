using MediatR;

namespace RSPeer.Application.Features.ScriptAccesses.Commands
{
	public class AddScriptAccessCommand : IRequest<Unit>
	{
		public int UserId { get; set; }
		public int ScriptId { get; set; }
		public int? AdminUserId { get; set; }
	}
}