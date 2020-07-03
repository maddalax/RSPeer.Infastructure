using MediatR;
using RSPeer.Application.Features.Discord.Models;

namespace RSPeer.Application.Features.Discord.Commands
{
	public class SendDiscordWebHookCommand : IRequest<Unit>
	{
		public string Message { get; set; }
		public DiscordWebHookType Type { get; set; }
		public bool Critical { get; set; }
	}
}