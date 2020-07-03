using MediatR;

namespace RSPeer.Application.Features.ScriptAccesses.Commands
{
	public class AddPrivateScriptAccessCommand : IRequest<Unit>
	{
		public int ScriptId { get; set; }
		public int UserId { get; set; }
		
		public int RequestingUserId { get; set; }
		
		public string Username { get; set; }
	}
}