using System;
using MediatR;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
    public class GetUserIdByLinkKeyQuery : IRequest<int>
    {
        public Guid LinkKey { get; set; }
    }
}