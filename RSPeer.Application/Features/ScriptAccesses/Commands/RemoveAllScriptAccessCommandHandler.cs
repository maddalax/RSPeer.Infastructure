using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.ScriptAccesses.Commands
{
	public class RemoveAllScriptAccessCommandHandler : IRequestHandler<RemoveAllScriptAccessCommand, Unit>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public RemoveAllScriptAccessCommandHandler(RsPeerContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task<Unit> Handle(RemoveAllScriptAccessCommand request, CancellationToken cancellationToken)
		{
			var script = await _mediator.Send(new GetFullScriptByIdQuery { ScriptId = request.ScriptId }, cancellationToken);
			_db.ScriptAccess.RemoveRange(_db.ScriptAccess.Where(w => w.ScriptId == request.ScriptId));
			script.TotalUsers = 0;
			_db.Scripts.Update(script);
			await _db.SaveChangesAsync(cancellationToken);
			return Unit.Value;
		}
	}
}