using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
    public class GetDiscordAccountQuery : IRequest<DiscordAccount>
    {
        public string DiscordId { get; set; }
        public string Username { get; set; }
    }
}