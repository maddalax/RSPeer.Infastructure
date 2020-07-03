using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Discord.Events;
using RSPeer.Application.Features.Discord.Setup.Base;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Application.Infrastructure.Caching.Commands;
using RSPeer.Persistence;
using StackExchange.Redis;

namespace RSPeer.Application.Features.Discord.Listeners
{
    public class DiscordCacheListener : INotificationHandler<DiscordMessageEvent>
    {
        private readonly IDiscordSocketClientProvider _provider;
        private readonly IRedisService _redis;
        private readonly IMediator _mediator;
        private readonly RsPeerContext _db;

        public DiscordCacheListener(IDiscordSocketClientProvider provider, IRedisService redis, IMediator mediator, RsPeerContext db)
        {
            _provider = provider;
            _redis = redis;
            _mediator = mediator;
            _db = db;
        }

        public async Task Handle(DiscordMessageEvent notification, CancellationToken cancellationToken)
        {
            if (!notification.IsDirector)
            {
                return;
            }

            if (!notification.Content.StartsWith("!cache"))
            {
                return;
            }

            var split = notification.Content.Split(" ");

            if (split.Length == 2 && split[1] == "help")
            {
                await PrintHelp(notification);
                return;
            }
            
            if (split.Length < 3)
            {
                await SendMessage(notification, "Invalid command, must be !cache [ACTION] [PARAMS]");
                return;
            }

            var action = split[1];

            switch (action)
            {
                case "clear":
                {
                    var param = split[2];
                    if (split.Length < 4)
                    {
                        await SendMessage(notification, $"Invalid command, must be !cache clear {param} [VALUE]");
                        return;
                    }

                    var value = string.Join(' ', split.Skip(3));
                
                    switch (param)
                    {
                        case "user":
                            await ClearUserCache(notification, value);
                            break;
                        case "script":
                            await ClearScriptCache(notification, value);
                            break;
                        case "file":
                            await ClearFileCache(notification, value);
                            break;
                    }

                    return;
                }
            }
        }

        private async Task ClearUserCache(DiscordMessageEvent notification, string value)
        {
            value = value.Trim();
            try
            {
                await _mediator.Send(new UpdateUserCacheCommand(value));
            }
            catch (Exception e)
            {
                await SendMessage(notification, e.Message);
                return;
            }
            
            await SendMessage(notification, $"Successfully cleared cache for " + value + ".");
        }

        private async Task ClearScriptCache(DiscordMessageEvent notification, string value)
        {
            if (value != "meta" && value != "content")
            {
                await SendMessage(notification,
                    "Invalid value, must be meta or content. Meta being the script information, content being the actual script jar itself.");
                return;
            }
            
            var ids = await _db.Scripts.Select(w => w.Id).ToListAsync();

            if (value == "meta")
            {
                var keys = ids.Select(w => (RedisKey) $"script_{w}_details").ToArray();
                await _redis.GetDatabase().KeyDeleteAsync(keys);
                await SendMessage(notification, $"Successfully cleared script meta cache for {ids.Count} scripts.");
            }

            else if (value == "content")
            {
                var keys = ids.Select(w => (RedisKey) $"{w}_content_bytes").ToArray();
                await _redis.GetDatabase().KeyDeleteAsync(keys);
                await SendMessage(notification, $"Successfully cleared script content cache for {ids.Count} scripts.");
            }
        }

        private async Task PrintHelp(DiscordMessageEvent notification)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"!cache clear user [USERNAME | EMAIL]");
            builder.AppendLine($"!cache clear script meta");
            builder.AppendLine($"!cache clear script content");
            builder.AppendLine("!cache clear file [NAME]");
            var message = builder.ToString();
            SendMessage(notification, message);
        }

        private async Task ClearFileCache(DiscordMessageEvent notification, string value)
        {
            value = value.Trim();
            var transaction = _redis.GetDatabase().CreateTransaction();
            transaction.KeyDeleteAsync($"file_{value}_latest_version");
            transaction.KeyDeleteAsync($"latest_file_version_{value}");
            await transaction.ExecuteAsync();
            await SendMessage(notification, $"Successfully cleared file cache for {value}.");
        }
        
        private async Task SendMessage(DiscordMessageEvent notification, string message)
        {
            var client = await _provider.Get();
            
            var channel = client?.GetGuild(notification.GuildId)?
                .GetTextChannel(notification.ChannelId);

            if (channel == null)
            {
                return;
            }
            
            await channel.SendMessageAsync(message);
        }
    }
}