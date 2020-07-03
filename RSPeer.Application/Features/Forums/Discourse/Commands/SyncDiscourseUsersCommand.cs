using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Forums.Discourse.Commands
{
    public class SyncDiscourseUsersCommand : IRequest<User>
    {
        public string Email { get; set; }
    }
}