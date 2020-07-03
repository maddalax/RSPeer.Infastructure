using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Instance.Queries;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Domain.Dtos;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.ScriptAccesses.Queries
{
    public class HasAccessToScriptQueryHandler : IRequestHandler<HasAccessToScriptQuery, bool>
    {
        private readonly RsPeerContext _db;
        private readonly IMediator _mediator;

        public HasAccessToScriptQueryHandler(RsPeerContext db, IMediator mediator)
        {
            _db = db;
            _mediator = mediator;
        }

        public async Task<bool> Handle(HasAccessToScriptQuery request, CancellationToken cancellationToken)
        {
            var script = await _mediator.Send(new GetScriptByIdQuery {ScriptId = request.ScriptId}, cancellationToken);

            if (script.Type == ScriptType.HiddenPublic)
            {
                return true;
            }

            if (!request.UserId.HasValue)
            {
                return false;
            }
            
            if (script.Type == ScriptType.Premium)
            {
                return await CheckPremiumAccess(request, script);
            }

            if (script.Type == ScriptType.Private)
            {
                return await CheckPrivateAccess(request);
            }

            return await _db.ScriptAccess
                .Where(w => w.ScriptId == request.ScriptId && w.UserId == request.UserId.Value)
                .AnyAsync(cancellationToken);
        }

        private async Task<bool> CheckPremiumAccess(HasAccessToScriptQuery request, ScriptDto script)
        {
            if (!request.UserId.HasValue)
            {
                return false;
            }

            var date = DateTimeOffset.UtcNow;

            var count = await _db.ScriptAccess
                .Where(w => w.ScriptId == script.Id && w.UserId == request.UserId && w.Expiration.HasValue &&
                            w.Expiration.Value > date).Select(w => w.Instances.GetValueOrDefault(1)).SumAsync();

            if (!script.Instances.HasValue)
            {
                return count > 0;
            }

            var clients = await _mediator.Send(new GetRunningClientsQuery {UserId = request.UserId.Value});
            
            var dic = clients.GroupBy(w => w.Tag)
                .ToDictionary(w => w.Key, c => c.OrderByDescending(w => w.LastUpdate).ToList());

            var realCount = dic.Count(w => w.Value.First().ScriptName == script.Name);

            return count > realCount;
        }

        private async Task<bool> CheckPrivateAccess(HasAccessToScriptQuery request)
        {
            if (!request.UserId.HasValue)
            {
                return false;
            }

            return await _mediator.Send(new HasPrivateScriptAccessQuery
            {
                ScriptId = request.ScriptId,
                UserId = request.UserId.Value
            });
        }
    }
}