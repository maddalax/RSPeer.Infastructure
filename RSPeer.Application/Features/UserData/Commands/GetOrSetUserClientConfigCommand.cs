using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserData.Commands
{
    public class GetOrSetUserClientConfigCommand : IRequest<UserClientConfig>
    {
        public int UserId { get; set; }
    }
}