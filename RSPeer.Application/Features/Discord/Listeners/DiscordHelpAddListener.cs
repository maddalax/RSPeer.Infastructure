using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Discord.Events;
using RSPeer.Application.Features.Discord.Setup.Base;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Discord.Listeners
{
    public class DiscordHelpAddListener : INotificationHandler<DiscordMessageEvent>
    {
        private readonly IDiscordSocketClientProvider _provider;
        private readonly RsPeerContext _db;
        private readonly IRedisService _redis;

        public DiscordHelpAddListener(IDiscordSocketClientProvider provider, RsPeerContext db, IRedisService redis)
        {
            _provider = provider;
            _db = db;
            _redis = redis;
        }

        public async Task Handle(DiscordMessageEvent notification, CancellationToken cancellationToken)
        {
            if (!notification.IsDirector || !notification.Content.StartsWith("!"))
            {
                return;
            }

            var client = await _provider.Get();
            var channel = client?.GetGuild(notification.GuildId)
                .GetTextChannel(notification.ChannelId);
            
            var split = notification.Content.Split(" ");
            
            if (split.Length < 3)
            {
                return;
            }
            var command = split[0];
            var key = split[1];
            var value = notification.Content
                .Substring(command.Length + key.Length + 2).Trim();

            var exists = await _db.SiteConfig.FirstOrDefaultAsync(w => w.Key == $"discord:bot:reply:{key}", cancellationToken: cancellationToken);

            if (command == "!add_reply")
            {
                if (exists != null)
                {
                    await channel.SendMessageAsync($"Reponse by {key} already exists. If you wish to change, please delete with !remove_reply");
                    return;
                }

                await _db.SiteConfig.AddAsync(new Domain.Entities.SiteConfig
                {
                    Key = $"discord:bot:reply:{key}",
                    Value = value
                }, cancellationToken);

                await _redis.Remove("discord:bot:replies");
                await _db.SaveChangesAsync(cancellationToken);
                await channel.SendMessageAsync($"Successfully added response {key}.");
            }

            if (command == "!remove_reply")
            {
                if (exists != null)
                {
                    _db.Remove(exists);
                    await _db.SaveChangesAsync(cancellationToken);
                    var redisKey = $"discord:bot:reply:{key}";
                    await _redis.Remove(redisKey);
                    await _redis.Remove("discord:bot:replies");
                    await channel.SendMessageAsync($"Successfully removed {key}.");
                }
            }
        }
    }
}