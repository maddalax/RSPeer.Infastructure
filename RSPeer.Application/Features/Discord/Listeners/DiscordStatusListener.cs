using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using RSPeer.Application.Features.Discord.Events;
using RSPeer.Application.Features.Discord.Helpers;
using RSPeer.Application.Features.Discord.Setup.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Discord.Listeners
{
    public class DiscordStatusListener : INotificationHandler<DiscordMessageEvent>
    {
        private readonly RsPeerContext _db;
        private readonly IDiscordSocketClientProvider _provider;

        public DiscordStatusListener(RsPeerContext db, IDiscordSocketClientProvider provider)
        {
            _db = db;
            _provider = provider;
        }

        public async Task Handle(DiscordMessageEvent notification, CancellationToken cancellationToken)
        {
            var message = notification.Content;
            if (!notification.IsDirector)
            {
                return;
            }

            if (notification.Channel == null || !notification.Content.StartsWith("!status"))
            {
                return;
            }

            var split = message.Split(" ");
            var client = await _provider.Get();
            var channel = client?.GetGuild(notification.GuildId).GetTextChannel(notification.ChannelId);

            if (channel == null)
            {
                return;
            }
            
            if (split.Length < 3)
            {
                await channel.SendMessageAsync(
                    "Invalid command, format must be: !status MESSAGE (true || false)");
                return;
            }

            var content = split.Take(split.Length - 1).Join(" ").Replace("!status", string.Empty).Trim();
            var toShow = split[split.Length - 1].Trim();
            
            if (string.IsNullOrEmpty(content))
            {
                await channel.SendMessageAsync(
                    "You must specify a message to show. If wanting to remove the message, just put false as the third parameter. Example: !status no message false");
                return;
            }

            if (!bool.TryParse(toShow, out var shouldShow))
            {
                await channel.SendMessageAsync("Third parameter must be true or false.");
                return;
            }

            var shouldShowConfig = await _db.SiteConfig.FirstOrDefaultAsync(w => w.Key == "status:message:enabled",
                cancellationToken: cancellationToken);
            var messageConfig = await _db.SiteConfig.FirstOrDefaultAsync(w => w.Key == "status:message",
                cancellationToken: cancellationToken);

            if (shouldShowConfig == null || messageConfig == null)
            {
                await channel.SendMessageAsync(
                    "Failed to find config by key status:message:enabled || status:message");
                return;
            }

            shouldShowConfig.Value = shouldShow ? bool.TrueString : bool.FalseString;
            messageConfig.Value = content;

            _db.SiteConfig.Update(shouldShowConfig);
            _db.SiteConfig.Update(messageConfig);

            await _db.SaveChangesAsync(cancellationToken);

            await channel.SendMessageAsync(
                $"Successfully updated status message to {content} and should show to {shouldShow}.");
        }
    }
}