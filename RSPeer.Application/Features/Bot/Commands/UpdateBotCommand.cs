using MediatR;
using Microsoft.AspNetCore.Http;

namespace RSPeer.Application.Features.Bot.Commands
{
	public class UpdateBotCommand : IRequest<Unit>
	{
		public IFormFile File { get; set; }
		public bool SilentUpdate { get; set; }
		
		public decimal Version { get; set; }
	}
}