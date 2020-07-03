using System.Collections.Generic;
using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Scripts.Queries.CreateScript
{
    public class GetPendingScriptMessagesQuery : IRequest<IEnumerable<PendingScriptMap>>
    {
        public User User { get; set; }
    }
}