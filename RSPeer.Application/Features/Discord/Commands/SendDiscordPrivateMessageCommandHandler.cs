using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RSPeer.Application.Features.Discord.Setup.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Discord.Commands
{
    public class SendDiscordPrivateMessageCommandHandler : IRequestHandler<SendDiscordPrivateMessageCommand, bool>
    {
        private readonly RsPeerContext _db;
        private readonly IDiscordSocketClientProvider _provider;
        private ulong _guild;

        public SendDiscordPrivateMessageCommandHandler(RsPeerContext db, IDiscordSocketClientProvider provider, IConfiguration _configuration)
        {
            _db = db;
            _provider = provider;
            _guild = _configuration.GetValue<ulong>("Discord:GuildId");
        }

        public async Task<bool> Handle(SendDiscordPrivateMessageCommand request, CancellationToken cancellationToken)
        {
            var client = request.Client ?? await _provider.Get();
            var guild = client.GetGuild(_guild);
            if (guild == null)
            {
                return false;
            }

            var user = await _db.DiscordAccounts.Where(w => w.UserId == request.UserId).FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                return false;
            }

            var discordUser = guild.GetUser(Convert.ToUInt64(user.DiscordUserId));

            if (discordUser == null)
            {
                return false;
            }

            try
            {
                var sent = await discordUser.SendMessageAsync(request.Message);

                if (sent != null && sent.Id != default && request.SendDisclaimer)
                {
                    await discordUser.SendMessageAsync(
                        "Don't want to receive alerts for expiring orders? Reply back with: " +
                        "!disable_alerts\nTo enable alerts, reply with !enable_alerts");
                }

                return sent != null && sent.Id != default;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}