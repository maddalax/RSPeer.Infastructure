using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Infrastructure.Caching.Commands
{
    public class UpdateUserCacheCommand : IRequest<Unit>
    {
        public UpdateUserCacheCommand(string usernameOrEmail)
        {
            UsernameOrEmail = usernameOrEmail;
        }
        public string UsernameOrEmail { get; }
    }
}