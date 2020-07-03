using MediatR;

namespace RSPeer.Application.Features.UserLogging.Commands
{
    public class UserLogCommand : IRequest<Unit>
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }
}