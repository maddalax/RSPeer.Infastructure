using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Discord.Events;
using RSPeer.Application.Features.Discord.Setup.Base;
using RSPeer.Application.Infrastructure.Caching.Base;

namespace RSPeer.Application.Features.Discord.Listeners
{
	public class DiscordBotStatsListener : INotificationHandler<DiscordMessageEvent>
	{
		private readonly IRedisService _redis;
		private readonly IDiscordSocketClientProvider _provider;

		public DiscordBotStatsListener(IRedisService redis, IDiscordSocketClientProvider provider)
		{
			_redis = redis;
			_provider = provider;
		}

		public async Task Handle(DiscordMessageEvent notification, CancellationToken cancellationToken)
		{
			var message = notification.Content;

			if (notification.IsBot)
			{
				return;
			}
			
			if (message != "!stats")
			{
				return;
			}

			if (!notification.InGuild)
			{
				return;
			}
			
			var total = await _redis.Get<long>("connected_client_count");
			var toSend = $"There are currently {total} clients online.";
			var client = await _provider.Get();
			var task = client?.GetGuild(notification.GuildId)?.GetTextChannel(notification.ChannelId)?.SendMessageAsync(toSend);
			if (task != null)
			{
				await task;
			}
		}
	}
}