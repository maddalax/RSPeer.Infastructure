using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Discord.Events;
using RSPeer.Application.Features.Discord.Setup.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Discord.Listeners
{
    public class DiscordInuvationListener : INotificationHandler<DiscordMessageEvent>
    {
        private readonly IDiscordSocketClientProvider _provider;
        private readonly RsPeerContext _db;

        public DiscordInuvationListener(IDiscordSocketClientProvider provider, RsPeerContext db)
        {
            _provider = provider;
            _db = db;
        }
        
        public async Task Handle(DiscordMessageEvent notification, CancellationToken cancellationToken)
        {
            if (!notification.IsDirector)
            {
                return;
            }

            if (!notification.Content.StartsWith("!modscript"))
            {
                return;
            }

            var split = notification.Content.Split(" ");

            var client = await _provider.Get();

            if (split.Length < 2)
            {
                await client.GetGuild(notification.GuildId)
                    .GetTextChannel(notification.ChannelId)
                    .SendMessageAsync("Commands: clear, clear HASH_HERE, list" );
                return;
            }

            var command = split[1];
            
            try
            {
                if (command == "clear")
                {
                    if (split.Length == 3)
                    {
                        var hash = split[2].Trim();
                        _db.Files.RemoveRange(_db.Files.Where(w => w.Name == $"inuvation_modscript_{hash}"));
                        await _db.SaveChangesAsync(cancellationToken);
                        await client.GetGuild(notification.GuildId)
                            .GetTextChannel(notification.ChannelId)
                            .SendMessageAsync($"Successfully removed inuvation modscript {hash}.");
                    }
                    else
                    {
                        _db.Files.RemoveRange(_db.Files.Where(w => w.Name.StartsWith("inuvation_modscript_")));
                        await _db.SaveChangesAsync(cancellationToken);
                        await client.GetGuild(notification.GuildId)
                            .GetTextChannel(notification.ChannelId)
                            .SendMessageAsync("Successfully removed inuvation modscripts.");
                    }
                }

                if (command == "list")
                {
                    var names = await _db.Files.Where(w => w.Name.StartsWith("inuvation_modscript_")).Select(w => w.Name).ToListAsync();
                    var formatted = names.Count == 0 ? "No modscripts have been saved." : string.Join(',', names);
                    await client.GetGuild(notification.GuildId)
                        .GetTextChannel(notification.ChannelId)
                        .SendMessageAsync(formatted);
                }
            }
            catch (Exception e)
            {
                await client.GetGuild(notification.GuildId)
                    .GetTextChannel(notification.ChannelId)
                    .SendMessageAsync(e.Message);
            }
        }
    }
}