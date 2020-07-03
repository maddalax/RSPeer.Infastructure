using MediatR;

namespace RSPeer.Application.Features.Scripts.Commands.CreateScript
{
	public class AddScripterInfoCommand : IRequest<Unit>
	{
		public int UserId { get; set; }
		public int GitlabId { get; set; }
	}
}