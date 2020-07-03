using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripts.Commands.CreateScript
{
	public class RemovePendingRequestsCommandHandler : IRequestHandler<RemovePendingRequestsCommand, Unit>
	{
		private readonly RsPeerContext _db;

		public RemovePendingRequestsCommandHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<Unit> Handle(RemovePendingRequestsCommand request, CancellationToken cancellationToken)
		{
			var pending = await _db.PendingScripts.Where(w => w.LiveScriptId == request.ScriptId)
				.Select(w => w.PendingScriptId).ToListAsync(cancellationToken: cancellationToken);
			_db.Scripts.RemoveRange(_db.Scripts.Where(w => pending.Contains(w.Id)));
			_db.RemoveRange(_db.PendingScripts.Where(w => pending.Contains(w.PendingScriptId)));
			await _db.SaveChangesAsync(cancellationToken);
			return Unit.Value;
		}
	}
}