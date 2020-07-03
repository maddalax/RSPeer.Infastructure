using System;
using System.Text.Json.Serialization;

namespace RSPeer.Domain.Entities
{
	public class DiscordAccount
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string DiscordUserId { get; set; }
		public string DiscordUsername { get; set; }
		public int Discriminator { get; set; }
		public bool Bot { get; set; }
		public Guid Link { get; set; }
		public DateTimeOffset DateVerified { get; set; }
		
		[JsonIgnore]
		public User User { get; set; }
	}
}