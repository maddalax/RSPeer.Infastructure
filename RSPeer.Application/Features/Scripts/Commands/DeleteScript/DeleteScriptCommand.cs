using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Scripts.Commands.DeleteScript
{
	public class DeleteScriptCommand : IRequest<Unit>
	{
		public int ScriptId { get; set; }
		public User Admin { get; set; }
	}
}