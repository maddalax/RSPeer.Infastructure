using MediatR;

namespace RSPeer.Application.Features.Discord.Events
{
	public class DiscordMessageEvent : INotification
	{
		public string Processor { get; set; }
		public ulong Id { get; set; }
		public bool IsDirector { get; set; }
		public bool IsTokenSeller { get; set; }
		public string Content { get; set; }
		
		public bool InGuild { get; set; }

		public string Channel { get; set; }
		
		public bool IsBot { get; set; }
		
		public string Username { get; set; }
		
		public string Discriminator { get; set; }
		
		public ulong UserId { get; set; }
		
		public ulong ChannelId { get; set; }

		public ulong GuildId { get; set; }
	}
}