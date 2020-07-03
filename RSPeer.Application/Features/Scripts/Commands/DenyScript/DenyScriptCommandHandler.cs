using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripts.Commands.DenyScript
{
    public class DenyScriptCommandHandler : IRequestHandler<DenyScriptCommand, Unit>
    {
        private readonly RsPeerContext _db;

        public DenyScriptCommandHandler(RsPeerContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(DenyScriptCommand request, CancellationToken cancellationToken)
        {
            var script = await _db.Scripts.Where(w => w.Id == request.ScriptId && w.Status == ScriptStatus.Pending)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            script.Status = ScriptStatus.Denied;
            var pending = await _db.PendingScripts.Where(w => w.PendingScriptId == request.ScriptId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            
            pending.Status = ScriptStatus.Denied;
            pending.Message = request.Reason;
            pending.DeniedBy = request.Admin?.Id;

            _db.Scripts.Update(script);
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}