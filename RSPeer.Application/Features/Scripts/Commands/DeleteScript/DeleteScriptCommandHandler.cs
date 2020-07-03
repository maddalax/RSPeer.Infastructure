using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.ScriptAccesses.Commands;
using RSPeer.Application.Features.Scripts.Commands.CreateScript;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripts.Commands.DeleteScript
{
	public class DeleteScriptCommandHandler : IRequestHandler<DeleteScriptCommand, Unit>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public DeleteScriptCommandHandler(RsPeerContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task<Unit> Handle(DeleteScriptCommand request, CancellationToken cancellationToken)
		{
			var script = await _db.Scripts.FirstOrDefaultAsync(w => w.Id == request.ScriptId, cancellationToken);

			if (script == null)
			{
				throw new NotFoundException("Script", request.ScriptId);
			}

			if (script.Status == ScriptStatus.Live)
			{
				await _mediator.Send(new RemovePendingRequestsCommand { ScriptId = script.Id }, cancellationToken);
			}
			
			_db.Remove(script);
			
			await _db.Data.AddAsync(new Data
			{
				Key = $"script:delete:{script.Id}:{script.Name}:by",
				Value = request.Admin.Id.ToString()
			}, cancellationToken);
			
			await _db.SaveChangesAsync(cancellationToken);
			return Unit.Value;
		}
	}
}