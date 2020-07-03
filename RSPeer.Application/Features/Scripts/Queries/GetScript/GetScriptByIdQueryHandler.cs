using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Domain.Dtos;

namespace RSPeer.Application.Features.Scripts.Queries.GetScript
{
    public class GetScriptByIdQueryHandler : IRequestHandler<GetScriptByIdQuery, ScriptDto>
    {
        private readonly IMediator _mediator;

        public GetScriptByIdQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ScriptDto> Handle(GetScriptByIdQuery request, CancellationToken cancellationToken)
        {
            var results = await _mediator.Send(new GetScriptsByIdsQuery {ScriptIds = new HashSet<int> {request.ScriptId}}, 
                cancellationToken);
            return results.FirstOrDefault();
        }
    }
}