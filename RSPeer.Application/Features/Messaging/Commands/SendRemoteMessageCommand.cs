using MediatR;

namespace RSPeer.Application.Features.Messaging.Commands
{
    public class SendRemoteMessageCommand : IRequest<Unit>
    {
        public int UserId { get; set; }
        public string Consumer { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
    }
}