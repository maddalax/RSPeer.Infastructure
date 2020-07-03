using MediatR;
using Microsoft.AspNetCore.Http;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Scripts.Commands.CreateScript
{
	public class CreatePrivateScriptCommand : IRequest<Unit>
	{
		public Script Script { get; set; }
		
		public User User { get; set; }
		public IFormFile File { get; set; }
	}
}