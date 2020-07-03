using MediatR;
using RSPeer.Application.Features.UserManagement.Users.Queries.Options;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
    public class GetUserBySessionOrApiKeyQuery : IRequest<User>
    { 
        public string Session { get; set; }
        public UserLookupOptions Options { get; set; } = new UserLookupOptions();
    }
}