using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.ScriptAccesses.Commands
{
	public class RemovePrivateScriptAccessCommandHandler : IRequestHandler<RemovePrivateScriptAccessCommand, Unit>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public RemovePrivateScriptAccessCommandHandler(RsPeerContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task<Unit> Handle(RemovePrivateScriptAccessCommand request, CancellationToken cancellationToken)
		{
			
			var script = await _mediator.Send(new GetScriptByIdQuery { ScriptId = request.ScriptId }, cancellationToken);

			if (script.AuthorId != request.RequestingUserId && script.AuthorId != request.UserId)
			{
				throw new Exception("Only the developer of this script may remove access.");
			}
			
			_db.PrivateScriptAccess.RemoveRange(_db.PrivateScriptAccess.Where(w => w.ScriptId == request.ScriptId && w.UserId == request.UserId));
			await _mediator.Send(new RemoveScriptAccessCommand { ScriptId = request.ScriptId, UserId = request.UserId }, cancellationToken);
			await _db.SaveChangesAsync(cancellationToken);
			return Unit.Value;
		}
	}
}