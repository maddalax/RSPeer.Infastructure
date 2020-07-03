using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Exceptions;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripts.Commands.DeleteScript
{
    public class DeletePrivateScriptCommandHandler : IRequestHandler<DeletePrivateScriptCommand, Unit>
    {
        private readonly RsPeerContext _db;

        public DeletePrivateScriptCommandHandler(RsPeerContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(DeletePrivateScriptCommand request, CancellationToken cancellationToken)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync(cancellationToken))
            {
                var script = await _db.Scripts.Where(w =>
                        w.Type == ScriptType.Private && w.Id == request.ScriptId && w.UserId == request.UserId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (script == null)
                {
                    throw new NotFoundException("PrivateScript", request.ScriptId);
                }

                _db.ScriptContents.RemoveRange(_db.ScriptContents.Where(w => w.ScriptId == script.Id));
                _db.Scripts.Remove(script);
                _db.PrivateScriptAccess.RemoveRange(_db.PrivateScriptAccess.Where(w => w.ScriptId == script.Id));
                await _db.SaveChangesAsync(cancellationToken);
                transaction.Commit();
                return Unit.Value;
            }
        }
    }
}