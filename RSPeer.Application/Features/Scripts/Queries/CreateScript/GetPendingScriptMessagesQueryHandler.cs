using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripts.Queries.CreateScript
{
    public class GetPendingScriptMessagesQueryHandler : IRequestHandler<GetPendingScriptMessagesQuery, IEnumerable<PendingScriptMap>>
    {
        private readonly RsPeerContext _db;

        public GetPendingScriptMessagesQueryHandler(RsPeerContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<PendingScriptMap>> Handle(GetPendingScriptMessagesQuery request, CancellationToken cancellationToken)
        {
            var userId = request.User.Id;
            return await _db.PendingScripts.Where(w => w.Message != null)
                .Include(w => w.PendingScript)
                .Where(w => w.PendingScript.UserId == userId).ToListAsync(cancellationToken: cancellationToken);
        }
    }
}