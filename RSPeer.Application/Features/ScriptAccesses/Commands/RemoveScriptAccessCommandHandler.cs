using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.ScriptAccesses.Commands
{
	public class RemoveScriptAccessCommandHandler : IRequestHandler<RemoveScriptAccessCommand, Unit>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public RemoveScriptAccessCommandHandler(RsPeerContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task<Unit> Handle(RemoveScriptAccessCommand request, CancellationToken cancellationToken)
		{
			var script = await _mediator.Send(new GetFullScriptByIdQuery { ScriptId = request.ScriptId }, cancellationToken);

			var records = _db.ScriptAccess
				.Where(w => w.ScriptId == request.ScriptId && w.UserId == request.UserId);

			if (!records.Any())
			{
				return Unit.Value;
			}
			
			_db.ScriptAccess.RemoveRange(records);
			script.TotalUsers = script.TotalUsers == 0 ? 0 : script.TotalUsers - 1;
			_db.Scripts.Update(script);
			await _db.SaveChangesAsync(cancellationToken);
			await _mediator.Send(new ClearScriptAccessCacheCommand { UserId = request.UserId }, cancellationToken);
			return Unit.Value;
		}
	}
}