using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Discord.Events;
using RSPeer.Application.Features.Discord.Setup.Base;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Discord.Listeners
{
    public class DiscordHelpListener : INotificationHandler<DiscordMessageEvent>
    {
        private readonly IDiscordSocketClientProvider _provider;
        private readonly RsPeerContext _db;
        private readonly IRedisService _redis;

        public DiscordHelpListener(IDiscordSocketClientProvider provider, RsPeerContext db, IRedisService redis)
        {
            _provider = provider;
            _db = db;
            _redis = redis;
        }

        public async Task Handle(DiscordMessageEvent notification, CancellationToken cancellationToken)
        {
            if (notification.IsBot)
            {
                return;
            }
            
            if (!notification.Content.StartsWith("!"))
            {
                return;
            }

            var command = notification.Content?.Split(" ")[0]?.Substring(1);

            if (command == null)
            {
                return;
            }
            
            var client = await _provider.Get();

            var channel = client?.GetGuild(notification.GuildId)?
                .GetTextChannel(notification.ChannelId);

            if (channel == null)
            {
                return;
            }

            if (command == "h")
            {
                await SendList(channel);
                return;
            }
            
            var key = $"discord:bot:reply:{command}";
            var exists = await _redis.GetString(key);
            
            
            if (exists != null)
            {
                await channel.SendMessageAsync(exists);
                return;
            }

            var db = await _db.SiteConfig.FirstOrDefaultAsync(w => w.Key == key, cancellationToken);
            if (db != null)
            {
                await _redis.Set(key, db.Value, TimeSpan.FromHours(1));
                await channel.SendMessageAsync(db.Value);
            }
        }

        private async Task SendList(SocketTextChannel channel)
        {
            var commands = new HashSet<string>();
            var all = await _redis.GetSet("discord:bot:replies");
            if (all.Count == 0)
            {
                var replies = await _db.SiteConfig.Where(w => w.Key.Contains("discord:bot:reply:")).ToListAsync();
                commands = replies.Select(w => w.Key.Replace("discord:bot:reply:", "")).ToHashSet();
                foreach (var command in commands)
                {
                    await _redis.AddToSet("discord:bot:replies", command, TimeSpan.FromHours(1));
                }
            }
            else
            {
                commands = all;
            }

            commands.Add("verify");
            commands.Add("stats");

            var builder = new StringBuilder("Possible commands:").AppendLine();
            foreach (var command in commands)
            {
                builder.AppendLine($"!{command}");
            }

            await channel.SendMessageAsync(builder.ToString());
        }
    }
}