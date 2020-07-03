using System.Collections.Generic;
using MediatR;
using RSPeer.Domain.Dtos;

namespace RSPeer.Application.Features.Scripts.Queries.GetScript
{
    public class GetScriptsByIdsQuery : IRequest<IEnumerable<ScriptDto>>
    {
        public HashSet<int> ScriptIds { get; set; }
    }
}