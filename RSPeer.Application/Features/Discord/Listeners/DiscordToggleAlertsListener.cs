using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Discord.Commands;
using RSPeer.Application.Features.Discord.Events;
using RSPeer.Application.Features.UserData.Commands;
using RSPeer.Application.Features.UserManagement.Users.Queries;

namespace RSPeer.Application.Features.Discord.Listeners
{
    public class DiscordToggleAlertsListener : INotificationHandler<DiscordMessageEvent>
    {
        private readonly IMediator _mediator;

        public DiscordToggleAlertsListener(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Handle(DiscordMessageEvent notification, CancellationToken cancellationToken)
        {
            var isPrivateMessage = notification.Channel == $"@{notification.Username}#{notification.Discriminator}";
            if (!isPrivateMessage)
            {
                return;
            }


            var acc = await _mediator.Send(new GetDiscordAccountQuery {DiscordId = notification.UserId.ToString()},
                cancellationToken);

            if (acc == null)
            {
                return;
            }

            switch (notification.Content)
            {
                case "!enable_alerts":
                    await _mediator.Send(
                        new SaveUserJsonDataCommand {Key = "discord_allow_alerts", UserId = acc.UserId, Value = true},
                        cancellationToken);
                    await _mediator.Send(new SendDiscordPrivateMessageCommand
                        {Message = "You have enabled RSPeer alerts.", UserId = acc.UserId, SendDisclaimer = false}, cancellationToken);
                    break;
                case "!disable_alerts":
                    await _mediator.Send(
                        new SaveUserJsonDataCommand {Key = "discord_allow_alerts", UserId = acc.UserId, Value = false},
                        cancellationToken);
                    await _mediator.Send(new SendDiscordPrivateMessageCommand
                        {Message = "You have disabled RSPeer alerts.", UserId = acc.UserId, SendDisclaimer = false});
                    break;
            }
        }
    }
}