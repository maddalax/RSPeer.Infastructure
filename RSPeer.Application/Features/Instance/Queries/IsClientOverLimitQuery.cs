using System;
using MediatR;

namespace RSPeer.Application.Features.Instance.Queries
{
    public class IsClientOverLimitQuery : IRequest<bool>
    {
        public int UserId { get; set; }
        public Guid Tag { get; set; }
    }
}