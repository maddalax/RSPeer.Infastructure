using System.Threading.Tasks;
using Discord.WebSocket;

namespace RSPeer.Application.Features.Discord.Setup.Base
{
	public interface IDiscordSocketClientProvider
	{
		Task<DiscordSocketClient> Get();
	}
}