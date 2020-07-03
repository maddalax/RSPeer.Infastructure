using MediatR;
using Microsoft.AspNetCore.Http;

namespace RSPeer.Application.Features.Bot.Commands
{
	public class ObfuscateBotCommand : IRequest<byte[]>
	{
		public IFormFile File { get; set; }
		public string Path { get; set; }
	}
}