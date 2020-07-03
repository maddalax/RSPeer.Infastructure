using Discord.WebSocket;
using MediatR;

namespace RSPeer.Application.Features.Discord.Commands
{
    public class SendDiscordPrivateMessageCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public DiscordSocketClient Client;

        public bool SendDisclaimer { get; set; } = true;
    }
}