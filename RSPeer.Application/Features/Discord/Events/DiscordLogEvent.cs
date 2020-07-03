using Discord;
using MediatR;

namespace RSPeer.Application.Features.Discord.Events
{
	public class DiscordLogEvent : INotification
	{
		public LogMessage Event { get; set; }
	}
}