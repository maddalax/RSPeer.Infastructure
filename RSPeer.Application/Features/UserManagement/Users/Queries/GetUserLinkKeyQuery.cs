using System;
using MediatR;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
    public class GetUserLinkKeyQuery : IRequest<Guid?>
    {
        public int UserId { get; set; }
    }
}