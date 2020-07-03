using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Discord.Events;
using RSPeer.Application.Features.Discord.Setup.Base;
using RSPeer.Application.Features.Migration.Queries;
using RSPeer.Application.Features.Store.Items.Queries;
using RSPeer.Application.Features.UserManagement.Users.Commands;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Common.Enums;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Discord.Listeners
{
	public class DiscordTokenSalesListener : INotificationHandler<DiscordMessageEvent>
	{
		private readonly IMediator _mediator;
		private readonly IDiscordSocketClientProvider _provider;
		private readonly RsPeerContext _db;

		public DiscordTokenSalesListener(IMediator mediator, IDiscordSocketClientProvider provider, RsPeerContext db)
		{
			_mediator = mediator;
			_provider = provider;
			_db = db;
		}

		public async Task Handle(DiscordMessageEvent notification, CancellationToken cancellationToken)
		{
			var message = notification.Content;
			
			if (!notification.IsTokenSeller)
			{
				return;
			}

			if (notification.Channel == null || notification.Channel != "tokens" || !notification.Content.StartsWith("!tokens"))
			{
				return;
			}
			
			var enabled = await _mediator.Send(new IsReadOnlyModeQuery(), cancellationToken);

			var client = await _provider.Get();
			var channel = client?.GetGuild(notification.GuildId).GetTextChannel(notification.ChannelId);

			if (channel == null)
			{
				return;
			}
			
			if (enabled)
			{
				await channel.SendMessageAsync("RSPeer is in read only mode due to maintenance, services will be restored shortly. For more information, check status on https://app.rspeer.org");
				return;
			}
			
			var split = message.Split(" ");

			if (split.Length != 3)
			{
				await channel.SendMessageAsync("Invalid command, format must be: !tokens email amount");
				return;
			}
			
			var email = split[1];
			var amount = split[2];

			if (!int.TryParse(amount, out var quantity) || quantity <= 0 || quantity >= 1000000)
			{
				await channel.SendMessageAsync("Invalid quantity. Must be greater than 0 and less than 1 million.");
				return;
			}

			var user = await _mediator.Send(new GetUserByEmailQuery { Email = email }, cancellationToken);

			if (user == null)
			{
				await channel.SendMessageAsync("Failed to find user by that email address.");
				return;
			}
			
			
			var item = await _mediator.Send(new GetItemBySkuQuery { Sku = "tokens" }, cancellationToken);

			using (var transaction = await _db.Database.BeginTransactionAsync(cancellationToken))
			{
				var order = new Order
				{
					ItemId = item.Id,
					Quantity = quantity,
					UserId = user.Id,
					Timestamp = DateTimeOffset.UtcNow,
					Status = OrderStatus.Completed,
					Total = quantity * item.Price
				};

				await _db.Orders.AddAsync(order, cancellationToken);
				await _db.SaveChangesAsync(cancellationToken);
			
				await _mediator.Send(new UserUpdateBalanceCommand
				{
					Reason = $"token:sale:{notification.Username}",
					Type = AddRemove.Add,
					UserId = user.Id,
					OrderId = order.Id,
					Amount = quantity
				}, cancellationToken);
				
				transaction.Commit();
			}

			user = await _mediator.Send(new GetUserByIdQuery { AllowCached = false, Id = user.Id }, cancellationToken);
			
			await channel.SendMessageAsync($"Successfully updated {user.Username} ({user.Email})'s balance by {quantity}. New balance is {user.Balance} tokens.");
		}
	}
}