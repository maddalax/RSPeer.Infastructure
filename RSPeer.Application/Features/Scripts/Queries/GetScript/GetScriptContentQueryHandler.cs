using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.ScriptAccesses.Queries;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripts.Queries.GetScript
{
	public class GetScriptContentQueryHandler : IRequestHandler<GetScriptContentQuery, ScriptContent>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public GetScriptContentQueryHandler(RsPeerContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task<ScriptContent> Handle(GetScriptContentQuery request, CancellationToken cancellationToken)
		{
			if (request.CheckAccess)
			{
				var access = await _mediator.Send(new HasAccessToScriptQuery { ScriptId = request.ScriptId, UserId = request.User?.Id }, cancellationToken);
				if (!access)
				{
					throw new AuthorizationException("You are not allowed to run additional instances of this script currently.");
				}				
			}
			
			return await _db.ScriptContents.FirstOrDefaultAsync(w => w.ScriptId == request.ScriptId,
				cancellationToken);
		}
	}
}