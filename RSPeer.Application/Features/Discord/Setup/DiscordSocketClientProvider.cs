using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using RSPeer.Application.Features.Discord.Setup.Base;

namespace RSPeer.Application.Features.Discord.Setup
{
	public class DiscordSocketClientProvider : IDiscordSocketClientProvider
	{
		private readonly IConfiguration _configuration;
		private DiscordSocketClient _client;

		public DiscordSocketClientProvider(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<DiscordSocketClient> Get()
		{
			if (_client != null)
			{
				return _client;
			}
			_client = new DiscordSocketClient();
			await _client.LoginAsync(TokenType.Bot,
				_configuration.GetValue<string>("Discord:BotToken"));
			await _client.StartAsync();
			return _client;
		}
	}
}