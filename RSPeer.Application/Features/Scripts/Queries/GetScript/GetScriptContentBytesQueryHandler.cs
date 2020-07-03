using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.ScriptAccesses.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripts.Queries.GetScript
{
    public class GetScriptContentBytesQueryHandler : IRequestHandler<GetScriptContentBytesQuery, byte[]>
    {
        private readonly RsPeerContext _db;
        private readonly IRedisService _redis;
        private readonly IMediator _mediator;

        public GetScriptContentBytesQueryHandler(RsPeerContext db, IRedisService redis, IMediator mediator)
        {
            _db = db;
            _redis = redis;
            _mediator = mediator;
        }

        public async Task<byte[]> Handle(GetScriptContentBytesQuery request, CancellationToken cancellationToken)
        {
            if (request.CheckAccess)
            {
                var access = await _mediator.Send(new HasAccessToScriptQuery { ScriptId = request.ScriptId, UserId = request.User?.Id }, cancellationToken);
                if (!access)
                {
                    throw new AuthorizationException("You are not allowed to run additional instances of this script currently.");
                }				
            }

            var bytes = await _redis.GetDatabase().StringGetAsync($"{request.ScriptId}_content_bytes");

            if (bytes.HasValue)
            {
                return (byte[]) bytes;
            }

            var fromDb = await _db.ScriptContents.Where(w => w.ScriptId == request.ScriptId).Select(w => w.Content)
                .FirstOrDefaultAsync(cancellationToken);

            await _redis.GetDatabase().StringSetAsync($"{request.ScriptId}_content_bytes", fromDb);

            return fromDb;
        }
    }
}