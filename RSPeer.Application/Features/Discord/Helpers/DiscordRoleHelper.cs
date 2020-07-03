using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace RSPeer.Application.Features.Discord.Helpers
{
	public class DiscordRoleHelper
	{
		private static readonly Dictionary<DiscordRole, string> DiscordRoleMap = new Dictionary<DiscordRole, string>
		{
			{ DiscordRole.Director, "Director" },
			{ DiscordRole.TokenSeller, "Token Seller" }
		};

		private readonly IConfiguration _configuration;

		public DiscordRoleHelper(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public bool InGuild(SocketMessage message)
		{
			return GetGuildId(message) == _configuration.GetValue<ulong>("Discord:GuildId");
		}

		public ulong GetGuildId(SocketMessage message)
		{
			if (!(message.Author is IGuildUser author))
			{
				return 0;
			}

			return author.GuildId;
		}
		
		public bool HasRole(DiscordRole role, SocketMessage message)
		{
			if (!(message.Author is IGuildUser author))
			{
				return false;
			}

			if (!InGuild(message))
			{
				return false;
			}

			if (message.Source != MessageSource.User || message.Author.IsBot || message.Author.IsWebhook)
			{
				return false;
			}

			if (author is SocketGuildUser user)
			{
				var name = DiscordRoleMap.ContainsKey(role) ? DiscordRoleMap[role] : null;
				return name != null && user.Roles.FirstOrDefault(w => w.Name == name) != null;
			}

			return false;
		}
	}

	public enum DiscordRole
	{
		Director,
		TokenSeller
	}
}