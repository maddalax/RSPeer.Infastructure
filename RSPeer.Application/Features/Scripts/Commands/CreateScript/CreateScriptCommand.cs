using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Scripts.Commands.CreateScript
{
	public class CreateScriptCommand : IRequest<int>
	{
		public User User { get; set; }
		public Script Script { get; set; }
		public bool Recompile { get; set; }
		public User Admin { get; set; }
	}
}