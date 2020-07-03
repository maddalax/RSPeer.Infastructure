using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using RSPeer.Application.Features.Discord.Events;
using RSPeer.Application.Features.Discord.Helpers;
using RSPeer.Application.Features.Discord.Setup.Base;
using RSPeer.Application.Features.Migration.Queries;
using RSPeer.Application.Features.SiteConfig.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Discord.Listeners
{
	public class DiscordUserVerificationListener : INotificationHandler<DiscordMessageEvent>
	{
		private readonly IDiscordSocketClientProvider _provider;
		private readonly IMediator _mediator;
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;

		public DiscordUserVerificationListener(IDiscordSocketClientProvider provider, IMediator mediator, RsPeerContext db, IRedisService redis)
		{
			_provider = provider;
			_mediator = mediator;
			_db = db;
			_redis = redis;
		}

		public async Task Handle(DiscordMessageEvent notification, CancellationToken cancellationToken)
		{
			var message = notification.Content;

			if (!message.StartsWith("!verify") || !message.Contains("!verify"))
			{
				return;
			}

			var isPrivateMessage = notification.Channel == $"@{notification.Username}#{notification.Discriminator}";

			if (notification.IsBot)
			{
				return;
			}

			if (notification.Channel != "discord-verification-bot" && !isPrivateMessage)
			{
				return;
			}

			var enabled = await _mediator.Send(new IsReadOnlyModeQuery(), cancellationToken);


			var client = await _provider.Get();
			var channel = client?.GetGuild(notification.GuildId)?.GetTextChannel(notification.ChannelId);

			if (!isPrivateMessage && channel == null)
			{
				return;
			}

			if (channel != null && enabled)
			{
				await channel.SendMessageAsync("RSPeer is in read only due to maintenance, services will be restored shortly. For more information, check status on https://app.rspeer.org");
				return;
			}

			if (!isPrivateMessage && message != "!verify")
			{
				if (!notification.IsDirector)
				{
					await channel.DeleteMessageAsync(notification.Id);
				}

				return;
			}

			var config = await _mediator.Send(new GetSiteConfigOrThrowCommand {Key = "discord:bot:verification:enabled" }, cancellationToken);

			if (!bool.Parse(config))
			{
				return;
			}


			if (!isPrivateMessage && message == "!verify")
			{
				var instructions = await _mediator.Send(new GetSiteConfigOrThrowCommand { Key = "discord:verification:message" }, cancellationToken);
				await client.GetUser(notification.UserId).SendMessageAsync(instructions);
				return;
			}

			await HandlePrivateMessage(client, notification, cancellationToken);
		}

		private async Task HandlePrivateMessage(DiscordSocketClient client, DiscordMessageEvent notification, CancellationToken cancellationToken)
		{
			var author = client.GetUser(notification.UserId);

			if (!notification.Content.StartsWith("!"))
			{
				return;
			}

			await author.SendMessageAsync(
				"Attempting to verify your discord account with RSPeer. Please wait.");

			var content = notification.Content;
			var split = content.Split(" ");

			if (split.Length < 3)
			{
				await author.SendMessageAsync("Invalid format, please format your message like the example.");
				return;
			}

			if(!Guid.TryParse(split[1], out var tag) || tag == Guid.Empty)
			{
				await author.SendMessageAsync("Invalid link key format, must match the same format: b9f87833-0fbe-45eb-a302-242aec84e7b2");
				return;
			}

			if (notification.IsBot)
			{
				await author.SendMessageAsync("You may not verify as a bot!");
				return;
			}

			if (await _db.DiscordAccounts.AnyAsync(w => w.DiscordUserId == notification.UserId.ToString(), cancellationToken))
			{
				await author.SendMessageAsync("You are already verified!");
				return;
			}

			if (await _db.DiscordAccounts.AnyAsync(w => w.Link == tag, cancellationToken))
			{
				await author.SendMessageAsync("This link key has already been used to verify a user.");
				return;
			}

			var username = split.Skip(2).Join(" ");

			var user = await _db.Users.FirstOrDefaultAsync(w => w.LinkKey.HasValue && w.LinkKey.Value == tag, cancellationToken);

			if (user == null)
			{
				await author.SendMessageAsync("User not found by that link key.");
				return;
			}

			if (user.Username != username)
			{
				await author.SendMessageAsync("The user with that link key does not match the username you have provided.");
				return;
			}

			if (!user.LinkKey.HasValue)
			{
				await author.SendMessageAsync("User some how does not have a link key? This should not happen.");
				return;
			}

			var discordAccount = new DiscordAccount
			{
				Bot = notification.IsBot,
				DateVerified = DateTimeOffset.UtcNow,
				DiscordUserId = notification.UserId.ToString(),
				DiscordUsername = notification.Username,
				Discriminator = Convert.ToInt32(notification.Discriminator),
				Link = user.LinkKey.Value,
				UserId = user.Id
			};

			await _db.DiscordAccounts.AddAsync(discordAccount, cancellationToken);
			await _db.SaveChangesAsync(cancellationToken);
			await _redis.Remove($"{user.Id}_instances_allowed");
			await _redis.Remove($"{user.Id}_instances_allowed_no_free");

			var finish = await _mediator.Send(new GetSiteConfigOrThrowCommand
			{
				Key = "discord:verification:finish"
			}, cancellationToken);

			await author.SendMessageAsync(finish);
		}
	}
}