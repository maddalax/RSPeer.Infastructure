using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Discord.Events;

namespace RSPeer.Application.Features.Discord.Listeners
{
	public class DiscordLogListener : INotificationHandler<DiscordLogEvent>
	{
		public async Task Handle(DiscordLogEvent notification, CancellationToken cancellationToken)
		{
			Console.WriteLine(notification.Event);
		}
	}
}