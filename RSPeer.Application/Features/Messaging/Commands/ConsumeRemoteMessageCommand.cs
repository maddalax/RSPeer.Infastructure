using MediatR;

namespace RSPeer.Application.Features.Messaging.Commands
{
    public class ConsumeRemoteMessageCommand : IRequest<Unit>
    {
        public int MessageId { get; set; }
        public int UserId { get; set; }
    }
}