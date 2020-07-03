using System.Collections.Generic;
using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Messaging.Queries
{
    public class GetRemoteMessagesQuery : IRequest<IEnumerable<RemoteMessage>>
    {
        public int UserId { get; set; }
        public string Consumer { get; set; }
    }
}